/*
 * restrictions
 *  - as real hardware is used, timing might need tuning. see:
 *      - Thread.sleep(...)
 *      - obj.wait(...)
 *  - missing functionality on certain devices, see:
 *      - if(rfc2217_server_nonstandard_baudrates)
 *      - if(usbSerialDriver instanceof ...)
 *
 */
package com.hoho.android.usbserial;

import android.content.Context;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.support.test.InstrumentationRegistry;
import android.support.test.runner.AndroidJUnit4;
import android.util.Log;

import com.hoho.android.usbserial.driver.CdcAcmSerialDriver;
import com.hoho.android.usbserial.driver.Ch34xSerialDriver;
import com.hoho.android.usbserial.driver.CommonUsbSerialPort;
import com.hoho.android.usbserial.driver.Cp21xxSerialDriver;
import com.hoho.android.usbserial.driver.FtdiSerialDriver;
import com.hoho.android.usbserial.driver.ProbeTable;
import com.hoho.android.usbserial.driver.ProlificSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;
import com.hoho.android.usbserial.util.SerialInputOutputManager;
import com.hoho.android.usbserial.util.TelnetWrapper;
import com.hoho.android.usbserial.util.UsbWrapper;

import org.junit.After;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TestRule;
import org.junit.rules.TestWatcher;
import org.junit.runner.Description;
import org.junit.runner.RunWith;

import java.io.IOException;
import java.util.Arrays;
import java.util.EnumSet;
import java.util.List;
import java.util.concurrent.Executors;

import static org.hamcrest.CoreMatchers.anyOf;
import static org.hamcrest.CoreMatchers.equalTo;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertNotEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;

@RunWith(AndroidJUnit4.class)
public class DeviceTest {
    private final static String  TAG = DeviceTest.class.getSimpleName();

    // testInstrumentationRunnerArguments configuration
    private static String  rfc2217_server_host;
    private static int     rfc2217_server_port = 2217;
    private static boolean rfc2217_server_nonstandard_baudrates;
    private static String  test_device_driver;
    private static int     test_device_port;

    private Context context;
    private UsbManager usbManager;
    UsbWrapper usb;
    static TelnetWrapper telnet;
    private boolean isCp21xxRestrictedPort = false; // second port of Cp2105 has limited dataBits, stopBits, parity

    @Rule
    public TestRule watcher = new TestWatcher() {
        protected void starting(Description description) {
            Log.i(TAG, "===== starting test: " + description.getMethodName()+ " =====");
        }
    };

    @BeforeClass
    public static void setUpFixture() throws Exception {
        rfc2217_server_host                  =                 InstrumentationRegistry.getArguments().getString("rfc2217_server_host");
        rfc2217_server_nonstandard_baudrates = Boolean.valueOf(InstrumentationRegistry.getArguments().getString("rfc2217_server_nonstandard_baudrates"));
        test_device_driver                   =                 InstrumentationRegistry.getArguments().getString("test_device_driver");
        test_device_port                     = Integer.valueOf(InstrumentationRegistry.getArguments().getString("test_device_port","0"));

        // postpone parts of fixture setup to first test, because exceptions are not reported for @BeforeClass
        // and test terminates with misleading 'Empty test suite'
        telnet = new TelnetWrapper(rfc2217_server_host, rfc2217_server_port);
    }

    @Before
    public void setUp() throws Exception {
        telnet.setUp();

        context = InstrumentationRegistry.getContext();
        usbManager = (UsbManager) context.getSystemService(Context.USB_SERVICE);
        List<UsbSerialDriver> availableDrivers = UsbSerialProber.getDefaultProber().findAllDrivers(usbManager);
        if(availableDrivers.isEmpty()) {
            ProbeTable customTable = new ProbeTable();
            customTable.addProduct(0x2342, 0x8036, CdcAcmSerialDriver.class); // arduino multiport cdc witch custom VID
            availableDrivers = new UsbSerialProber(customTable).findAllDrivers(usbManager);
        }
        assertEquals("no USB device found", 1, availableDrivers.size());
        UsbSerialDriver usbSerialDriver = availableDrivers.get(0);
        if(test_device_driver != null) {
            String driverName = usbSerialDriver.getClass().getSimpleName();
            assertEquals(test_device_driver+"SerialDriver", driverName);
        }
        assertTrue( usbSerialDriver.getPorts().size() > test_device_port);
        usb = new UsbWrapper(context, usbSerialDriver, test_device_port);
        usb.setUp();

        Log.i(TAG, "Using USB device "+ usb.serialPort.toString()+" driver="+usb.serialDriver.getClass().getSimpleName());
        isCp21xxRestrictedPort = usb.serialDriver instanceof Cp21xxSerialDriver && usb.serialDriver.getPorts().size()==2 && test_device_port == 1;
        telnet.read(-1); // doesn't look related here, but very often after usb permission dialog the first test failed with telnet garbage
    }

    @After
    public void tearDown() throws IOException {
        if(usb != null)
            usb.tearDown();
        telnet.tearDown();
    }

    @AfterClass
    public static void tearDownFixture() throws Exception {
        telnet.tearDownFixture();
    }

    private static class TestBuffer {
        private byte[] buf;
        private int len;

        private TestBuffer(int length) {
            len = 0;
            buf = new byte[length];
            int i=0;
            int j=0;
            for(j=0; j<length/16; j++)
                for(int k=0; k<16; k++)
                    buf[i++]=(byte)j;
            while(i<length)
                buf[i++]=(byte)j;
        }

        private boolean testRead(byte[] data) {
            assertNotEquals(0, data.length);
            assertTrue("got " + (len+data.length) +" bytes", (len+data.length) <= buf.length);
            for(int j=0; j<data.length; j++)
                assertEquals("at pos "+(len+j), (byte)((len+j)/16), data[j]);
            len += data.length;
            //Log.d(TAG, "read " + len);
            return len == buf.length;
        }
    }


    // clone of org.apache.commons.lang3.StringUtils.indexOfDifference + optional startpos
    private static int indexOfDifference(final CharSequence cs1, final CharSequence cs2) {
        return indexOfDifference(cs1, cs2, 0, 0);
    }

    private static int indexOfDifference(final CharSequence cs1, final CharSequence cs2, int cs1startpos, int cs2startpos) {
        if (cs1 == cs2) {
            return -1;
        }
        if (cs1 == null || cs2 == null) {
            return 0;
        }
        if(cs1startpos < 0 || cs2startpos < 0)
            return -1;
        int i, j;
        for (i = cs1startpos, j = cs2startpos; i < cs1.length() && j < cs2.length(); ++i, ++j) {
            if (cs1.charAt(i) != cs2.charAt(j)) {
                break;
            }
        }
        if (j < cs2.length() || i < cs1.length()) {
            return i;
        }
        return -1;
    }

    private int findDifference(final StringBuilder data, final StringBuilder expected) {
        int length = 0;
        int datapos = indexOfDifference(data, expected);
        int expectedpos = datapos;
        while(datapos != -1) {
            int nextexpectedpos = -1;
            int nextdatapos = datapos + 2;
            int len = -1;
            if(nextdatapos + 10 < data.length()) { // try to sync data+expected, assuming that data is lost, but not corrupted
                String nextsub = data.substring(nextdatapos, nextdatapos + 10);
                nextexpectedpos = expected.indexOf(nextsub, expectedpos);
                if(nextexpectedpos >= 0) {
                    len = nextexpectedpos - expectedpos - 2;
                }
            }
            Log.i(TAG, "difference at " + datapos + " len " + len );
            Log.d(TAG, "       got " +     data.substring(Math.max(datapos - 20, 0), Math.min(datapos + 20, data.length())));
            Log.d(TAG, "  expected " + expected.substring(Math.max(expectedpos - 20, 0), Math.min(expectedpos + 20, expected.length())));
            datapos = indexOfDifference(data, expected, nextdatapos, nextexpectedpos);
            expectedpos = nextexpectedpos + (datapos  - nextdatapos);
            if(len==-1) length=-1;
            else        length+=len;
        }
        return length;
    }

    private void doReadWrite(String reason) throws Exception {
        byte[] buf1 = new byte[]{ 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16};
        byte[] buf2 = new byte[]{ 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26};
        byte[] data;

        telnet.write(buf1);
        data = usb.read(buf1.length);
        assertThat(reason, data, equalTo(buf1)); // includes array content in output
        //assertArrayEquals("net2usb".getBytes(), data); // only includes array length in output
        usb.write(buf2);
        data = telnet.read(buf2.length);
        assertThat(reason, data, equalTo(buf2));
    }

    @Test
    public void openClose() throws Exception {
        usb.open();
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        doReadWrite("");

        try {
            usb.serialPort.open(usb.deviceConnection);
            fail("already open expected");
        } catch (IOException ignored) {
        }
        doReadWrite("");

        usb.close();
        try {
            usb.serialPort.close();
            fail("already closed expected");
        } catch (IOException ignored) {
        }
        try {
            usb.write(new byte[]{0x00});
            fail("write error expected");
        } catch (IOException ignored) {
        }
        try {
            usb.read(1);
            fail("read error expected");
        } catch (IOException ignored) {
        }
        try {
            usb.setParameters(9600, 8, 1, UsbSerialPort.PARITY_NONE);
            fail("error expected");
        } catch (IOException ignored) {
        } catch (NullPointerException ignored) {
        }

        usb.open();
        telnet.setParameters(9600, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.setParameters(9600, 8, 1, UsbSerialPort.PARITY_NONE);
        doReadWrite("");

        // close port before iomanager
        assertEquals(SerialInputOutputManager.State.RUNNING, usb.ioManager.getState());
        usb.serialPort.close();
        for (int i = 0; i < 1000; i++) {
            if (usb.ioManager.getState() == SerialInputOutputManager.State.STOPPED)
                break;
            Thread.sleep(1);
        }
        // assertEquals(SerialInputOutputManager.State.STOPPED, usb.usbIoManager.getState());
        // unstable. null'ify not-stopped ioManager, else usbClose would try again
        if(SerialInputOutputManager.State.STOPPED != usb.ioManager.getState())
            usb.ioManager = null;
    }

    @Test
    public void baudRate() throws Exception {
        usb.open();

        if (false) { // default baud rate
            // CP2102: only works if first connection after attaching device
            // PL2303, FTDI: it's not 9600
            telnet.setParameters(9600, 8, 1, UsbSerialPort.PARITY_NONE);

            doReadWrite("");
        }

        // invalid values
        try {
            usb.setParameters(-1, 8, 1, UsbSerialPort.PARITY_NONE);
            fail("invalid baud rate");
        } catch (IllegalArgumentException ignored) {
        }
        try {
            usb.setParameters(0, 8, 1, UsbSerialPort.PARITY_NONE);
            fail("invalid baud rate");
        } catch (IllegalArgumentException ignored) {
        }
        try {
            usb.setParameters(1, 8, 1, UsbSerialPort.PARITY_NONE);
            if (usb.serialDriver instanceof FtdiSerialDriver)
                ;
            else if (usb.serialDriver instanceof ProlificSerialDriver)
                ;
            else if (usb.serialDriver instanceof Cp21xxSerialDriver)
                ;
            else if (usb.serialDriver instanceof CdcAcmSerialDriver)
                ;
            else
                fail("invalid baudrate 1");
        } catch (UnsupportedOperationException ignored) { // ch340
        } catch (IOException ignored) { // cp2105 second port
        } catch (IllegalArgumentException ignored) {
        }
        try {
            usb.setParameters(2<<31, 8, 1, UsbSerialPort.PARITY_NONE);
            if (usb.serialDriver instanceof ProlificSerialDriver)
                ;
            else if (usb.serialDriver instanceof Cp21xxSerialDriver)
                ;
            else if (usb.serialDriver instanceof CdcAcmSerialDriver)
                ;
            else
                fail("invalid baudrate 2^31");
        } catch (ArithmeticException ignored) { // ch340
        } catch (IOException ignored) { // cp2105 second port
        } catch (IllegalArgumentException ignored) {
        }

        for(int baudRate : new int[] {300, 2400, 19200, 115200} ) {
            if(baudRate == 300 && isCp21xxRestrictedPort) {
                try {
                    usb.setParameters(baudRate, 8, 1, UsbSerialPort.PARITY_NONE);
                    fail("baudrate 300 on cp21xx restricted port");
                } catch (IOException ignored) {
                }
                continue;
            }
            telnet.setParameters(baudRate, 8, 1, UsbSerialPort.PARITY_NONE);
            usb.setParameters(baudRate, 8, 1, UsbSerialPort.PARITY_NONE);

            doReadWrite(baudRate+"/8N1");
        }
        if(rfc2217_server_nonstandard_baudrates && !isCp21xxRestrictedPort) {
            // usbParameters does not fail on devices that do not support nonstandard baud rates
            usb.setParameters(42000, 8, 1, UsbSerialPort.PARITY_NONE);
            telnet.setParameters(42000, 8, 1, UsbSerialPort.PARITY_NONE);

            byte[] buf1 = "abc".getBytes();
            byte[] buf2 = "ABC".getBytes();
            byte[] data1, data2;
            usb.write(buf1);
            data1 = telnet.read();
            telnet.write(buf2);
            data2 = usb.read();
            if (usb.serialDriver instanceof ProlificSerialDriver) {
                // not supported
                assertNotEquals(data1, buf2);
                assertNotEquals(data2, buf2);
            } else if (usb.serialDriver instanceof Cp21xxSerialDriver) {
                if (usb.serialDriver.getPorts().size() > 1) {
                    // supported on cp2105 first port
                    assertThat("42000/8N1", data1, equalTo(buf1));
                    assertThat("42000/8N1", data2, equalTo(buf2));
                } else {
                    // not supported on cp2102
                    assertNotEquals(data1, buf1);
                    assertNotEquals(data2, buf2);
                }
            } else {
                assertThat("42000/8N1", data1, equalTo(buf1));
                assertThat("42000/8N1", data2, equalTo(buf2));
            }
        }
        { // non matching baud rate
            telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
            usb.setParameters(2400, 8, 1, UsbSerialPort.PARITY_NONE);

            byte[] data;
            telnet.write("net2usb".getBytes());
            data = usb.read();
            assertNotEquals(7, data.length);
            usb.write("usb2net".getBytes());
            data = telnet.read();
            assertNotEquals(7, data.length);
        }
    }

    @Test
    public void dataBits() throws Exception {
        byte[] data;

        usb.open();
        for(int i: new int[] {0, 4, 9}) {
            try {
                usb.setParameters(19200, i, 1, UsbSerialPort.PARITY_NONE);
                fail("invalid databits "+i);
            } catch (IllegalArgumentException ignored) {
            }
        }

        // telnet -> usb
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 7, 1, UsbSerialPort.PARITY_NONE);
        telnet.write(new byte[] {0x00});
        Thread.sleep(10); // one bit is 0.05 milliseconds long, wait >> stop bit
        telnet.write(new byte[] {(byte)0xff});
        data = usb.read(2);
        assertThat("19200/7N1", data, equalTo(new byte[] {(byte)0x80, (byte)0xff}));

        telnet.setParameters(19200, 6, 1, UsbSerialPort.PARITY_NONE);
        telnet.write(new byte[] {0x00});
        Thread.sleep(10);
        telnet.write(new byte[] {(byte)0xff});
        data = usb.read(2);
        assertThat("19000/6N1", data, equalTo(new byte[] {(byte)0xc0, (byte)0xff}));

        telnet.setParameters(19200, 5, 1, UsbSerialPort.PARITY_NONE);
        telnet.write(new byte[] {0x00});
        Thread.sleep(10);
        telnet.write(new byte[] {(byte)0xff});
        data = usb.read(2);
        assertThat("19000/5N1", data, equalTo(new byte[] {(byte)0xe0, (byte)0xff}));

        // usb -> telnet
        try {
            telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
            usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_NONE);
            usb.write(new byte[]{0x00});
            Thread.sleep(10);
            usb.write(new byte[]{(byte) 0xff});
            data = telnet.read(2);
            assertThat("19000/7N1", data, equalTo(new byte[]{(byte) 0x80, (byte) 0xff}));
        } catch (UnsupportedOperationException e) {
                if(!isCp21xxRestrictedPort)
                    throw e;
        }
        try {
            usb.setParameters(19200, 6, 1, UsbSerialPort.PARITY_NONE);
            usb.write(new byte[]{0x00});
            Thread.sleep(10);
            usb.write(new byte[]{(byte) 0xff});
            data = telnet.read(2);
            assertThat("19000/6N1", data, equalTo(new byte[]{(byte) 0xc0, (byte) 0xff}));
        } catch (UnsupportedOperationException e) {
            if (!(isCp21xxRestrictedPort || usb.serialDriver instanceof FtdiSerialDriver))
                throw e;
        }
        try {
            usb.setParameters(19200, 5, 1, UsbSerialPort.PARITY_NONE);
            usb.write(new byte[] {0x00});
            Thread.sleep(5);
            usb.write(new byte[] {(byte)0xff});
            data = telnet.read(2);
            assertThat("19000/5N1", data, equalTo(new byte[] {(byte)0xe0, (byte)0xff}));
        } catch (UnsupportedOperationException e) {
            if (!(isCp21xxRestrictedPort || usb.serialDriver instanceof FtdiSerialDriver))
                throw e;
        }
    }

    @Test
    public void parity() throws Exception {
        byte[] _8n1 = {(byte)0x00, (byte)0x01, (byte)0xfe, (byte)0xff};
        byte[] _7n1 = {(byte)0x00, (byte)0x01, (byte)0x7e, (byte)0x7f};
        byte[] _7o1 = {(byte)0x80, (byte)0x01, (byte)0xfe, (byte)0x7f};
        byte[] _7e1 = {(byte)0x00, (byte)0x81, (byte)0x7e, (byte)0xff};
        byte[] _7m1 = {(byte)0x80, (byte)0x81, (byte)0xfe, (byte)0xff};
        byte[] _7s1 = {(byte)0x00, (byte)0x01, (byte)0x7e, (byte)0x7f};
        byte[] data;

        usb.open();
        for(int i: new int[] {-1, 5}) {
            try {
                usb.setParameters(19200, 8, 1, i);
                fail("invalid parity "+i);
            } catch (IllegalArgumentException ignored) {
            }
        }
        if(isCp21xxRestrictedPort) {
            usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
            usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_EVEN);
            usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_ODD);
            try {
                usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_MARK);
                fail("parity mark");
            } catch (UnsupportedOperationException ignored) {}
            try {
                usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_SPACE);
                fail("parity space");
            } catch (UnsupportedOperationException ignored) {}
            return;
            // test below not possible as it requires unsupported 7 dataBits
        }

        // usb -> telnet
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.write(_8n1);
        data = telnet.read(4);
        assertThat("19200/8N1", data, equalTo(_8n1));

        usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_ODD);
        usb.write(_8n1);
        data = telnet.read(4);
        assertThat("19200/7O1", data, equalTo(_7o1));

        usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_EVEN);
        usb.write(_8n1);
        data = telnet.read(4);
        assertThat("19200/7E1", data, equalTo(_7e1));

        if (usb.serialDriver instanceof CdcAcmSerialDriver) {
            // not supported by arduino_leonardo_bridge.ino, other devices might support it
            usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_MARK);
            usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_SPACE);
        } else {
            usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_MARK);
            usb.write(_8n1);
            data = telnet.read(4);
            assertThat("19200/7M1", data, equalTo(_7m1));

            usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_SPACE);
            usb.write(_8n1);
            data = telnet.read(4);
            assertThat("19200/7S1", data, equalTo(_7s1));
        }

        // telnet -> usb
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.write(_8n1);
        data = usb.read(4);
        assertThat("19200/8N1", data, equalTo(_8n1));

        telnet.setParameters(19200, 7, 1, UsbSerialPort.PARITY_ODD);
        telnet.write(_8n1);
        data = usb.read(4);
        assertThat("19200/7O1", data, equalTo(_7o1));

        telnet.setParameters(19200, 7, 1, UsbSerialPort.PARITY_EVEN);
        telnet.write(_8n1);
        data = usb.read(4);
        assertThat("19200/7E1", data, equalTo(_7e1));

        if (usb.serialDriver instanceof CdcAcmSerialDriver) {
            // not supported by arduino_leonardo_bridge.ino, other devices might support it
        } else {
            telnet.setParameters(19200, 7, 1, UsbSerialPort.PARITY_MARK);
            telnet.write(_8n1);
            data = usb.read(4);
            assertThat("19200/7M1", data, equalTo(_7m1));

            telnet.setParameters(19200, 7, 1, UsbSerialPort.PARITY_SPACE);
            telnet.write(_8n1);
            data = usb.read(4);
            assertThat("19200/7S1", data, equalTo(_7s1));

            usb.setParameters(19200, 7, 1, UsbSerialPort.PARITY_ODD);
            telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
            telnet.write(_8n1);
            data = usb.read(4);
            assertThat("19200/8N1", data, equalTo(_7n1)); // read is resilient against errors
        }
    }

    @Test
    public void stopBits() throws Exception {
        byte[] data;

        usb.open();
        for (int i : new int[]{0, 4}) {
            try {
                usb.setParameters(19200, 8, i, UsbSerialPort.PARITY_NONE);
                fail("invalid stopbits " + i);
            } catch (IllegalArgumentException ignored) {
            }
        }

        if (usb.serialDriver instanceof CdcAcmSerialDriver) {
            usb.setParameters(19200, 8, UsbSerialPort.STOPBITS_1_5, UsbSerialPort.PARITY_NONE);
            // software based bridge in arduino_leonardo_bridge.ino is to slow for real test, other devices might support it
        } else {
            // shift stopbits into next byte, by using different databits
            // a - start bit (0)
            // o - stop bit  (1)
            // d - data bit

            // out 8N2:   addddddd doaddddddddo
            //             1000001 0  10001111
            // in 6N1:    addddddo addddddo
            //             100000   101000
            usb.setParameters(19200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);
            telnet.setParameters(19200, 6, 1, UsbSerialPort.PARITY_NONE);
            usb.write(new byte[]{(byte)0x41, (byte)0xf1});
            data = telnet.read(2);
            assertThat("19200/8N1", data, equalTo(new byte[]{1, 5}));

            // out 8N2:   addddddd dooaddddddddoo
            //             1000001 0   10011111
            // in 6N1:    addddddo addddddo
            //             100000   110100
            try {
                usb.setParameters(19200, 8, UsbSerialPort.STOPBITS_2, UsbSerialPort.PARITY_NONE);
                telnet.setParameters(19200, 6, 1, UsbSerialPort.PARITY_NONE);
                usb.write(new byte[]{(byte) 0x41, (byte) 0xf9});
                data = telnet.read(2);
                assertThat("19200/8N1", data, equalTo(new byte[]{1, 11}));
            } catch(UnsupportedOperationException e) {
                if(!isCp21xxRestrictedPort)
                    throw e;
            }
            try {
                usb.setParameters(19200, 8, UsbSerialPort.STOPBITS_1_5, UsbSerialPort.PARITY_NONE);
                // todo: could create similar test for 1.5 stopbits, by reading at double speed
                //       but only some devices support 1.5 stopbits and it is basically not used any more
            } catch(UnsupportedOperationException ignored) {
            }
        }
    }


    @Test
    public void probeTable() throws Exception {
        class DummyDriver implements UsbSerialDriver {
            @Override
            public UsbDevice getDevice() { return null; }
            @Override
            public List<UsbSerialPort> getPorts() { return null; }
        }
        List<UsbSerialDriver> availableDrivers;
        ProbeTable probeTable = new ProbeTable();
        UsbManager usbManager = (UsbManager) context.getSystemService(Context.USB_SERVICE);
        availableDrivers = new UsbSerialProber(probeTable).findAllDrivers(usbManager);
        assertEquals(0, availableDrivers.size());

        probeTable.addProduct(0, 0, DummyDriver.class);
        availableDrivers = new UsbSerialProber(probeTable).findAllDrivers(usbManager);
        assertEquals(0, availableDrivers.size());

        probeTable.addProduct(usb.serialDriver.getDevice().getVendorId(), usb.serialDriver.getDevice().getProductId(), usb.serialDriver.getClass());
        availableDrivers = new UsbSerialProber(probeTable).findAllDrivers(usbManager);
        assertEquals(1, availableDrivers.size());
        assertEquals(availableDrivers.get(0).getClass(), usb.serialDriver.getClass());
    }

    @Test
    public void writeTimeout() throws Exception {
        usb.open();
        usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);

        // Basically all devices have a UsbEndpoint.getMaxPacketSize() 64. When the timeout
        // in usb.serialPort.write() is reached, some packets have been written and the rest
        // is discarded. bulkTransfer() does not return the number written so far, but -1.
        // With 115200 baud and 1/2 second timeout, typical values are:
        //   ch340    6080 of 6144
        //   pl2302   5952 of 6144
        //   cp2102   6400 of 7168
        //   cp2105   6272 of 7168
        //   ft232    5952 of 6144
        //   ft2232   9728 of 10240
        //   arduino   128 of 144
        int timeout = 500;
        int len = 0;
        int startLen = 1024;
        int step = 1024;
        int minLen = 4069;
        int maxLen = 12288;
        int bufferSize = 511;
        TestBuffer buf = new TestBuffer(len);
        if(usb.serialDriver instanceof CdcAcmSerialDriver) {
            startLen = 16;
            step = 16;
            minLen = 128;
            maxLen = 256;
            bufferSize = 31;
        }

        try {
            for (len = startLen; len < maxLen; len += step) {
                buf = new TestBuffer(len);
                Log.d(TAG, "write buffer size " + len);
                usb.serialPort.write(buf.buf, timeout);
                while (!buf.testRead(telnet.read(-1)))
                    ;
            }
            fail("write timeout expected between " + minLen + " and " + maxLen + ", is " + len);
        } catch (IOException e) {
            Log.d(TAG, "usbWrite failed", e);
            while (true) {
                byte[] data = telnet.read(-1);
                if (data.length == 0) break;
                if (buf.testRead(data)) break;
            }
            Log.d(TAG, "received " + buf.len + " of " + len + " bytes of failing usbWrite");
            assertTrue("write timeout expected between " + minLen + " and " + maxLen + ", is " + len, len > minLen);
        }

        // With smaller writebuffer, the timeout is used per bulkTransfer and each call 'fits'
        // into this timout, but shouldn't further calls only use the remaining timeout?
        ((CommonUsbSerialPort) usb.serialPort).setWriteBufferSize(bufferSize);
        len = maxLen;
        buf = new TestBuffer(len);
        Log.d(TAG, "write buffer size " + len);
        usb.serialPort.write(buf.buf, timeout);
        while (!buf.testRead(telnet.read(-1)))
            ;
    }

    @Test
    public void writeFragments() throws Exception {
        usb.open();
        usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);

        ((CommonUsbSerialPort) usb.serialPort).setWriteBufferSize(12);
        ((CommonUsbSerialPort) usb.serialPort).setWriteBufferSize(12); // keeps last buffer
        TestBuffer buf = new TestBuffer(256);
        usb.serialPort.write(buf.buf, 5000);
        while (!buf.testRead(telnet.read(-1)))
            ;
    }

    @Test
    // provoke data loss, when data is not read fast enough
    public void readBufferOverflow() throws Exception {
        if(usb.serialDriver instanceof CdcAcmSerialDriver)
            telnet.writeDelay = 10; // arduino_leonardo_bridge.ino sends each byte in own USB packet, which is horribly slow
        usb.open();
        usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);

        StringBuilder expected = new StringBuilder();
        StringBuilder data = new StringBuilder();
        final int maxWait = 2000;
        int bufferSize;
        for(bufferSize = 8; bufferSize < (2<<15); bufferSize *= 2) {
            int linenr;
            String line="-";
            expected.setLength(0);
            data.setLength(0);

            Log.i(TAG, "bufferSize " + bufferSize);
            usb.readBlock = true;
            for (linenr = 0; linenr < bufferSize/8; linenr++) {
                line = String.format("%07d,", linenr);
                telnet.write(line.getBytes());
                expected.append(line);
            }
            usb.readBlock = false;

            // slowly write new data, until old data is completely read from buffer and new data is received
            boolean found = false;
            for (; linenr < bufferSize/8 + maxWait/10 && !found; linenr++) {
                line = String.format("%07d,", linenr);
                telnet.write(line.getBytes());
                Thread.sleep(10);
                expected.append(line);
                data.append(new String(usb.read(0)));
                found = data.toString().endsWith(line);
            }
            while(!found) {
                // use waiting read to clear input queue, else next test would see unexpected data
                byte[] rest = usb.read(-1);
                if(rest.length == 0)
                    fail("last line "+line+" not found");
                data.append(new String(rest));
                found = data.toString().endsWith(line);
            }
            if (data.length() != expected.length())
                break;
        }

        findDifference(data, expected);
        assertTrue(bufferSize > 16);
        assertTrue(data.length() != expected.length());
    }

    @Test
    public void readSpeed() throws Exception {
        // see logcat for performance results
        //
        // CDC arduino_leonardo_bridge.ini has transfer speed ~ 100 byte/sec
        // all other devices are near physical limit with ~ 10-12k/sec
        //
        // readBufferOverflow provokes read errors, but they can also happen here where the data is actually read fast enough.
        // Android is not a real time OS, so there is no guarantee that the USB thread is scheduled, or it might be blocked by Java garbage collection.
        // Using SERIAL_INPUT_OUTPUT_MANAGER_THREAD_PRIORITY=THREAD_PRIORITY_URGENT_AUDIO sometimes reduced errors by factor 10, sometimes not at all!
        //
        int diffLen = readSpeedInt(5, 0);
        if(usb.serialDriver instanceof Ch34xSerialDriver && diffLen == -1)
             diffLen = 0; // todo: investigate last packet loss
        assertEquals(0, diffLen);
    }

    private int readSpeedInt(int writeSeconds, int readTimeout) throws Exception {
        int baudrate = 115200;
        if(usb.serialDriver instanceof Ch34xSerialDriver)
            baudrate = 38400;
        int writeAhead = 5*baudrate/10; // write ahead for another 5 second read
        if(usb.serialDriver instanceof CdcAcmSerialDriver)
            writeAhead = 50;

        usb.open(EnumSet.noneOf(UsbWrapper.OpenCloseFlags.class), readTimeout);
        usb.setParameters(baudrate, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(baudrate, 8, 1, UsbSerialPort.PARITY_NONE);

        int linenr = 0;
        String line="";
        StringBuilder data = new StringBuilder();
        StringBuilder expected = new StringBuilder();
        int dlen = 0, elen = 0;
        Log.i(TAG, "readSpeed: 'read' should be near "+baudrate/10);
        long begin = System.currentTimeMillis();
        long next = System.currentTimeMillis();
        for(int seconds=1; seconds <= writeSeconds; seconds++) {
            next += 1000;
            while (System.currentTimeMillis() < next) {
                if((writeAhead < 0) || (expected.length() < data.length() + writeAhead)) {
                    line = String.format("%07d,", linenr++);
                    telnet.write(line.getBytes());
                    expected.append(line);
                } else {
                    Thread.sleep(0, 100000);
                }
                data.append(new String(usb.read(0)));
            }
            Log.i(TAG, "readSpeed: t="+(next-begin)+", read="+(data.length()-dlen)+", write="+(expected.length()-elen));
            dlen = data.length();
            elen = expected.length();
        }

        boolean found = false;
        while(!found) {
            // use waiting read to clear input queue, else next test would see unexpected data
            byte[] rest = usb.read(-1);
            if(rest.length == 0)
                break;
            data.append(new String(rest));
            found = data.toString().endsWith(line);
        }
        return findDifference(data, expected);
    }

    @Test
    public void writeSpeed() throws Exception {
        // see logcat for performance results
        //
        // CDC arduino_leonardo_bridge.ini has transfer speed ~ 100 byte/sec
        // all other devices can get near physical limit:
        // longlines=true:, speed is near physical limit at 11.5k
        // longlines=false: speed is 3-4k for all devices, as more USB packets are required
        usb.open();
        usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
        boolean longlines = !(usb.serialDriver instanceof CdcAcmSerialDriver);

        int linenr = 0;
        String line="";
        StringBuilder data = new StringBuilder();
        StringBuilder expected = new StringBuilder();
        int dlen = 0, elen = 0;
        Log.i(TAG, "writeSpeed: 'write' should be near "+115200/10);
        long begin = System.currentTimeMillis();
        long next = System.currentTimeMillis();
        for(int seconds=1; seconds<=5; seconds++) {
            next += 1000;
            while (System.currentTimeMillis() < next) {
                if(longlines)
                    line = String.format("%060d,", linenr++);
                else
                    line = String.format("%07d,", linenr++);
                usb.write(line.getBytes());
                expected.append(line);
                data.append(new String(telnet.read(0)));
            }
            Log.i(TAG, "writeSpeed: t="+(next-begin)+", write="+(expected.length()-elen)+", read="+(data.length()-dlen));
            dlen = data.length();
            elen = expected.length();
        }
        boolean found = false;
        for (linenr=0; linenr < 2000 && !found; linenr++) {
            data.append(new String(telnet.read(0)));
            Thread.sleep(1);
            found = data.toString().endsWith(line);
        }
        next = System.currentTimeMillis();
        Log.i(TAG, "writeSpeed: t="+(next-begin)+", read="+(data.length()-dlen));
        assertTrue(found);
        int pos = indexOfDifference(data, expected);
        if(pos!=-1) {

            Log.i(TAG, "writeSpeed: first difference at " + pos);
            String datasub     =     data.substring(Math.max(pos - 20, 0), Math.min(pos + 20, data.length()));
            String expectedsub = expected.substring(Math.max(pos - 20, 0), Math.min(pos + 20, expected.length()));
            assertThat(datasub, equalTo(expectedsub));
        }
    }

    @Test
    public void purgeHwBuffers() throws Exception {
        // purge write buffer
        // 2400 is slowest baud rate for isCp21xxRestrictedPort
        usb.open();
        usb.setParameters(2400, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(2400, 8, 1, UsbSerialPort.PARITY_NONE);
        byte[] buf = new byte[64];
        for(int i=0; i<buf.length; i++) buf[i]='a';
        StringBuilder data = new StringBuilder();

        usb.write(buf);
        Thread.sleep(50); // ~ 12 bytes
        boolean purged = usb.serialPort.purgeHwBuffers(true, false);
        usb.write("bcd".getBytes());
        Thread.sleep(50);
        while(data.length()==0 || data.charAt(data.length()-1)!='d')
            data.append(new String(telnet.read()));
        Log.i(TAG, "purgeHwBuffers " + purged + ": " + (buf.length+3) + " -> " + data.length());

        assertTrue(data.length() > 5);
        if(purged) {
            if(usb.serialDriver instanceof Cp21xxSerialDriver && usb.serialDriver.getPorts().size() == 1) // only working on some devices/ports
                assertTrue(data.length() < buf.length + 1 || data.length() == buf.length + 3);
            else
                assertTrue(data.length() < buf.length + 1);
        } else {
            assertEquals(data.length(), buf.length + 3);
        }

        // purge read buffer
        usb.close();
        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.write("x".getBytes());
        Thread.sleep(10); // ~ 20 bytes
        purged = usb.serialPort.purgeHwBuffers(false, true);
        Log.d(TAG, "purged = " + purged);
        telnet.write("y".getBytes());
        Thread.sleep(10); // ~ 20 bytes
        if(purged) {
            if(usb.serialDriver instanceof Cp21xxSerialDriver) { // only working on some devices/ports
                if(isCp21xxRestrictedPort) {
                    assertThat(usb.read(2), equalTo("xy".getBytes())); // cp2105/1
                } else if(usb.serialDriver.getPorts().size() > 1) {
                    assertThat(usb.read(1), equalTo("y".getBytes()));  // cp2105/0
                } else {
                    assertThat(usb.read(2), anyOf(equalTo("xy".getBytes()), // cp2102
                                                                equalTo("y".getBytes()))); // cp2102
                }
            } else {
                assertThat(usb.read(1), equalTo("y".getBytes()));
            }
        } else {
            assertThat(usb.read(2), equalTo("xy".getBytes()));
        }
    }

    @Test
    public void writeAsync() throws Exception {
        if (usb.serialDriver instanceof FtdiSerialDriver)
            return; // periodically sends status messages, so does not block here

        byte[] data, buf = new byte[]{1};

        usb.ioManager = new SerialInputOutputManager(null);
        assertEquals(null, usb.ioManager.getListener());
        usb.ioManager.setListener(usb);
        assertEquals(usb, usb.ioManager.getListener());
        usb.ioManager = new SerialInputOutputManager(usb.serialPort, usb);
        assertEquals(usb, usb.ioManager.getListener());
        assertEquals(0, usb.ioManager.getReadTimeout());
        usb.ioManager.setReadTimeout(100);
        assertEquals(100, usb.ioManager.getReadTimeout());
        assertEquals(0, usb.ioManager.getWriteTimeout());
        usb.ioManager.setWriteTimeout(200);
        assertEquals(200, usb.ioManager.getWriteTimeout());

        // w/o timeout: write delayed until something is read
        usb.open();
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.ioManager.writeAsync(buf);
        usb.ioManager.writeAsync(buf);
        data = telnet.read(1);
        assertEquals(0, data.length);
        telnet.write(buf);
        data = usb.read(1);
        assertEquals(1, data.length);
        data = telnet.read(2);
        assertEquals(2, data.length);
        try {
            usb.ioManager.setReadTimeout(100);
            fail("IllegalStateException expected");
        } catch (IllegalStateException ignored) {}
        usb.close();

        // with timeout: write after timeout
        usb.open(EnumSet.noneOf(UsbWrapper.OpenCloseFlags.class), 100);
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.ioManager.writeAsync(buf);
        usb.ioManager.writeAsync(buf);
        data = telnet.read(2);
        assertEquals(2, data.length);
        usb.ioManager.setReadTimeout(200);
    }

    @Test
    public void readTimeout() throws Exception {
        if (usb.serialDriver instanceof FtdiSerialDriver)
            return; // periodically sends status messages, so does not block here
        final Boolean[] closed = {Boolean.FALSE};

        Runnable closeThread = new Runnable() {
            @Override
            public void run() {
                try {
                    Thread.sleep(100);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                usb.close();
                closed[0] = true;
            }
        };

        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);

        byte[] buf = new byte[]{1};
        int len,i,j;
        long time;

        // w/o timeout
        telnet.write(buf);
        len = usb.serialPort.read(buf, 0); // not blocking because data is available
        assertEquals(1, len);

        time = System.currentTimeMillis();
        closed[0] = false;
        Executors.newSingleThreadExecutor().submit(closeThread);
        len = usb.serialPort.read(buf, 0); // blocking until close()
        assertEquals(0, len);
        assertTrue(System.currentTimeMillis()-time >= 100);
        // wait for usbClose
        for(i=0; i<100; i++) {
            if(closed[0]) break;
            Thread.sleep(1);
        }
        assertTrue("not closed in time", closed[0]);

        // with timeout
        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);

        int longTimeout = 1000;
        int shortTimeout = 10;
        time = System.currentTimeMillis();
        len = usb.serialPort.read(buf, shortTimeout);
        assertEquals(0, len);
        assertTrue(System.currentTimeMillis()-time < 100);

        // no issue with slow transfer rate and short read timeout
        time = System.currentTimeMillis();
        for(i=0; i<50; i++) {
            Thread.sleep(10);
            telnet.write(buf);
            for(j=0; j<20; j++) {
                len = usb.serialPort.read(buf, shortTimeout);
                if (len > 0)
                    break;
            }
            assertEquals("failed after " + i, 1, len);
        }
        Log.i(TAG, "average time per read " + (System.currentTimeMillis()-time)/i + " msec");

        if(!(usb.serialDriver instanceof CdcAcmSerialDriver)) {
            int diffLen;
            usb.close();
            // no issue with high transfer rate and long read timeout
            diffLen = readSpeedInt(5, longTimeout);
            if(usb.serialDriver instanceof Ch34xSerialDriver && diffLen == -1)
                diffLen = 0; // todo: investigate last packet loss
            assertEquals(0, diffLen);
            usb.close();
            // date loss with high transfer rate and short read timeout !!!
            diffLen = readSpeedInt(5, shortTimeout);

            assertNotEquals(0, diffLen);

            // data loss observed with read timeout up to 200 msec, e.g.
            //  difference at 181 len 64
            //        got 000020,0000021,0000030,0000031,0000032,0
            //   expected 000020,0000021,0000022,0000023,0000024,0
            // difference at 341 len 128
            //        got 000048,0000049,0000066,0000067,0000068,0
            //   expected 000048,0000049,0000050,0000051,0000052,0
            // difference at 724 len 704
            //        got 0000112,0000113,0000202,0000203,0000204,
            //   expected 0000112,0000113,0000114,0000115,0000116,
            // difference at 974 len 8
            //        got 00231,0000232,0000234,0000235,0000236,00
            //   expected 00231,0000232,0000233,0000234,0000235,00
        }
    }

    @Test
    public void wrongDriver() throws Exception {

        UsbDeviceConnection wrongDeviceConnection;
        UsbSerialDriver wrongSerialDriver;
        UsbSerialPort wrongSerialPort;

        if(!(usb.serialDriver instanceof CdcAcmSerialDriver)) {
            wrongDeviceConnection = usbManager.openDevice(usb.serialDriver.getDevice());
            wrongSerialDriver = new CdcAcmSerialDriver(usb.serialDriver.getDevice());
            wrongSerialPort = wrongSerialDriver.getPorts().get(0);
            try {
                wrongSerialPort.open(wrongDeviceConnection);
                wrongSerialPort.setParameters(115200, UsbSerialPort.DATABITS_8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE); // ch340 fails here
                wrongSerialPort.write(new byte[]{1}, 1000); // pl2302 does not fail, but sends with wrong baud rate
                if(!(usb.serialDriver instanceof ProlificSerialDriver))
                    fail("error expected");
            } catch (IOException ignored) {
            }
            try {
                if(usb.serialDriver instanceof ProlificSerialDriver) {
                    assertNotEquals(new byte[]{1}, telnet.read());
                }
                wrongSerialPort.close();
                if(!(usb.serialDriver instanceof Ch34xSerialDriver |
                     usb.serialDriver instanceof ProlificSerialDriver))
                    fail("error expected");
            } catch (IOException ignored) {
            }
        }
        if(!(usb.serialDriver instanceof Ch34xSerialDriver)) {
            wrongDeviceConnection = usbManager.openDevice(usb.serialDriver.getDevice());
            wrongSerialDriver = new Ch34xSerialDriver(usb.serialDriver.getDevice());
            wrongSerialPort = wrongSerialDriver.getPorts().get(0);
            try {
                wrongSerialPort.open(wrongDeviceConnection);
                fail("error expected");
            } catch (IOException ignored) {
            }
            try {
                wrongSerialPort.close();
                fail("error expected");
            } catch (IOException ignored) {
            }
        }
        // FTDI only recovers from Cp21xx control commands with power toggle, so skip this combination!
        if(!(usb.serialDriver instanceof Cp21xxSerialDriver | usb.serialDriver instanceof FtdiSerialDriver)) {
            wrongDeviceConnection = usbManager.openDevice(usb.serialDriver.getDevice());
            wrongSerialDriver = new Cp21xxSerialDriver(usb.serialDriver.getDevice());
            wrongSerialPort = wrongSerialDriver.getPorts().get(0);
            try {
                wrongSerialPort.open(wrongDeviceConnection);
                //if(usb.usbSerialDriver instanceof FtdiSerialDriver)
                //    wrongSerialPort.setParameters(115200, UsbSerialPort.DATABITS_8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE); // ch340 fails here
                fail("error expected");
            } catch (IOException ignored) {
            }
            try {
                wrongSerialPort.close();
                //if(!(usb.usbSerialDriver instanceof FtdiSerialDriver))
                //    fail("error expected");
            } catch (IOException ignored) {
            }
        }
        if(!(usb.serialDriver instanceof FtdiSerialDriver)) {
            wrongDeviceConnection = usbManager.openDevice(usb.serialDriver.getDevice());
            wrongSerialDriver = new FtdiSerialDriver(usb.serialDriver.getDevice());
            wrongSerialPort = wrongSerialDriver.getPorts().get(0);
            try {
                wrongSerialPort.open(wrongDeviceConnection);
                if(usb.serialDriver instanceof Cp21xxSerialDriver)
                    wrongSerialPort.setParameters(115200, UsbSerialPort.DATABITS_8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE); // ch340 fails here
                //fail("error expected"); // only fails on some devices
            } catch (IOException ignored) {
            }
            try {
                wrongSerialPort.close();
                if(!(usb.serialDriver instanceof Cp21xxSerialDriver))
                    fail("error expected");
            } catch (IOException ignored) {
            }
        }
        if(!(usb.serialDriver instanceof ProlificSerialDriver)) {
            wrongDeviceConnection = usbManager.openDevice(usb.serialDriver.getDevice());
            wrongSerialDriver = new ProlificSerialDriver(usb.serialDriver.getDevice());
            wrongSerialPort = wrongSerialDriver.getPorts().get(0);
            try {
                wrongSerialPort.open(wrongDeviceConnection);
                fail("error expected");
            } catch (IOException ignored) {
            }
            try {
                wrongSerialPort.close();
                fail("error expected");
            } catch (IOException ignored) {
            }
        }
        // test that device recovers from wrong commands
        usb.open();
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        doReadWrite("");
    }

    @Test
    /* test not done by RFC2217 server. Instead output control lines are connected to
         input control lines with a binary decoder 74LS138, 74LS139, 74HC... or ...
        in
            A0 = RTS
            A1 = DTR
        out
            Y0 = CD
            Y1 = DTS/DSR
            Y2 = CTS
            Y3 = RI
        expected result:
            none -> RI
            RTS  -> CTS
            DTR  -> DTS/DSR
            both -> CD
     */
    public void controlLines() throws Exception {
        byte[] data;
        int sleep = 10;

        // output lines are supported by all drivers
        // input lines are supported by all drivers except CDC
        boolean inputLinesSupported = false;
        boolean inputLinesConnected = false;
        if (usb.serialDriver instanceof FtdiSerialDriver) {
            inputLinesSupported = true;
            inputLinesConnected = usb.serialDriver.getPorts().size() == 2; // I only have 74LS138 connected at FT2232
        } else if (usb.serialDriver instanceof Cp21xxSerialDriver) {
            inputLinesSupported = true;
            inputLinesConnected = usb.serialDriver.getPorts().size()==1; // I only have 74LS138 connected at CP2102
        } else if (usb.serialDriver instanceof ProlificSerialDriver) {
            inputLinesSupported = true;
            inputLinesConnected = true;
        } else if (usb.serialDriver instanceof Ch34xSerialDriver) {
            inputLinesSupported = true;
            inputLinesConnected = true;
        }
        EnumSet<UsbSerialPort.ControlLine> supportedControlLines = EnumSet.of(UsbSerialPort.ControlLine.RTS, UsbSerialPort.ControlLine.DTR);
        if(inputLinesSupported) {
            supportedControlLines.add(UsbSerialPort.ControlLine.CTS);
            supportedControlLines.add(UsbSerialPort.ControlLine.DSR);
            supportedControlLines.add(UsbSerialPort.ControlLine.CD);
            supportedControlLines.add(UsbSerialPort.ControlLine.RI);
        }

        // UsbSerialProber creates new UsbSerialPort objects which resets control lines,
        // so the initial open has the output control lines unset.
        // On additional close+open the output control lines can be retained.
        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_CONTROL_LINE_INIT));
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        Thread.sleep(sleep);

        assertEquals(supportedControlLines, usb.serialPort.getSupportedControlLines());
        if(usb.serialDriver instanceof ProlificSerialDriver) {
            // the initial status is sometimes not available or wrong.
            // this is more likely if other tests have been executed before.
            // start thread and wait until status hopefully updated.
            usb.serialPort.getRI(); // todo
            Thread.sleep(sleep);
            assertTrue(usb.serialPort.getRI());
        }

        // control lines reset on initial open
        data = "none".getBytes();
        assertEquals(inputLinesConnected
                        ? EnumSet.of(UsbSerialPort.ControlLine.RI)
                        : EnumSet.noneOf(UsbSerialPort.ControlLine.class),
                usb.serialPort.getControlLines());
        assertFalse(usb.serialPort.getRTS());
        assertFalse(usb.serialPort.getCTS());
        assertFalse(usb.serialPort.getDTR());
        assertFalse(usb.serialPort.getDSR());
        assertFalse(usb.serialPort.getCD());
        assertEquals(usb.serialPort.getRI(), inputLinesConnected);
        telnet.write(data);
        if(usb.serialDriver instanceof CdcAcmSerialDriver)
            // arduino: control line feedback as serial_state notification is not implemented.
            // It does not send w/o RTS or DTR, so these control lines can be partly checked here.
            assertEquals(0, usb.read().length);
        else
            assertThat(Arrays.toString(data), usb.read(4), equalTo(data));
        usb.write(data);
        assertThat(Arrays.toString(data), telnet.read(4), equalTo(data));

        data = "rts ".getBytes();
        usb.serialPort.setRTS(true);
        Thread.sleep(sleep);
        assertEquals(inputLinesConnected
                        ? EnumSet.of(UsbSerialPort.ControlLine.RTS, UsbSerialPort.ControlLine.CTS)
                        : EnumSet.of(UsbSerialPort.ControlLine.RTS),
                usb.serialPort.getControlLines());
        assertTrue(usb.serialPort.getRTS());
        assertEquals(usb.serialPort.getCTS(), inputLinesConnected);
        assertFalse(usb.serialPort.getDTR());
        assertFalse(usb.serialPort.getDSR());
        assertFalse(usb.serialPort.getCD());
        assertFalse(usb.serialPort.getRI());
        telnet.write(data);
        assertThat(Arrays.toString(data), usb.read(4), equalTo(data));
        usb.write(data);
        assertThat(Arrays.toString(data), telnet.read(4), equalTo(data));

        data = "both".getBytes();
        usb.serialPort.setDTR(true);
        Thread.sleep(sleep);
        assertEquals(inputLinesConnected
                        ? EnumSet.of(UsbSerialPort.ControlLine.RTS, UsbSerialPort.ControlLine.DTR, UsbSerialPort.ControlLine.CD)
                        : EnumSet.of(UsbSerialPort.ControlLine.RTS, UsbSerialPort.ControlLine.DTR),
                usb.serialPort.getControlLines());
        assertTrue(usb.serialPort.getRTS());
        assertFalse(usb.serialPort.getCTS());
        assertTrue(usb.serialPort.getDTR());
        assertFalse(usb.serialPort.getDSR());
        assertEquals(usb.serialPort.getCD(), inputLinesConnected);
        assertFalse(usb.serialPort.getRI());
        telnet.write(data);
        assertThat(Arrays.toString(data), usb.read(4), equalTo(data));
        usb.write(data);
        assertThat(Arrays.toString(data), telnet.read(4), equalTo(data));

        data = "dtr ".getBytes();
        usb.serialPort.setRTS(false);
        Thread.sleep(sleep);
        assertEquals(inputLinesConnected
                        ? EnumSet.of(UsbSerialPort.ControlLine.DTR, UsbSerialPort.ControlLine.DSR)
                        : EnumSet.of(UsbSerialPort.ControlLine.DTR),
                usb.serialPort.getControlLines());
        assertFalse(usb.serialPort.getRTS());
        assertFalse(usb.serialPort.getCTS());
        assertTrue(usb.serialPort.getDTR());
        assertEquals(usb.serialPort.getDSR(), inputLinesConnected);
        assertFalse(usb.serialPort.getCD());
        assertFalse(usb.serialPort.getRI());
        telnet.write(data);
        assertThat(Arrays.toString(data), usb.read(4), equalTo(data));
        usb.write(data);
        assertThat(Arrays.toString(data), telnet.read(4), equalTo(data));

        // control lines retained over close+open
        boolean inputRetained = inputLinesConnected;
        boolean outputRetained = true;
        if(usb.serialDriver instanceof FtdiSerialDriver)
            outputRetained = false; // todo
        usb.close(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_CONTROL_LINE_INIT));
        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_CONTROL_LINE_INIT, UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);

        EnumSet<UsbSerialPort.ControlLine> retainedControlLines = EnumSet.noneOf(UsbSerialPort.ControlLine.class);
        if(outputRetained) retainedControlLines.add(UsbSerialPort.ControlLine.DTR);
        if(inputRetained)  retainedControlLines.add(UsbSerialPort.ControlLine.DSR);
        assertEquals(retainedControlLines, usb.serialPort.getControlLines());
        assertFalse(usb.serialPort.getRTS());
        assertFalse(usb.serialPort.getCTS());
        assertEquals(usb.serialPort.getDTR(), outputRetained);
        assertEquals(usb.serialPort.getDSR(), inputRetained);
        assertFalse(usb.serialPort.getCD());
        assertFalse(usb.serialPort.getRI());

        usb.close(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_CONTROL_LINE_INIT));
        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_CONTROL_LINE_INIT, UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.setParameters(19200, 8, 1, UsbSerialPort.PARITY_NONE);
        for (int i = 0; i < usb.serialDriver.getDevice().getInterfaceCount(); i++)
            usb.deviceConnection.releaseInterface(usb.serialDriver.getDevice().getInterface(i));
        usb.deviceConnection.close();

        // set... error
        try {
            usb.serialPort.setRTS(true);
            fail("error expected");
        } catch (IOException ignored) {
        }

        // get... error
        try {
            usb.serialPort.getRI();
            if (!inputLinesSupported)
                ;
            else if (usb.serialDriver instanceof ProlificSerialDriver)
                ; // todo: currently not possible to detect, as bulkTransfer in background thread does not distinguish timeout and error
            else
                fail("error expected");
        } catch (IOException ignored) {
        }
    }

    @Test
    public void deviceConnection() throws Exception {
        byte buf[] = new byte[256];
        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);

        usb.write("x".getBytes());
        usb.serialPort.read(buf, 1000);
        usb.serialPort.setRTS(true);
        usb.serialPort.getRI();
        boolean purged = usb.serialPort.purgeHwBuffers(true, true);

        usb.deviceConnection.close();
        try {
            usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
            if(!(usb.serialDriver instanceof ProlificSerialDriver))
                fail("setParameters error expected");
        } catch (IOException ignored) {
        }
        try {
            usb.write("x".getBytes());
            fail("write error expected");
        } catch (IOException ignored) {
        }
        usb.serialPort.read(buf, 1000); // bulkTransfer returns -1 on timeout and error, so no exception thrown here
        try {
            usb.serialPort.read(buf, 0);
            fail("read error expected");
        } catch (IOException ignored) {
        }
        try {
            usb.serialPort.setRTS(true);
            fail("setRts error expected");
        } catch (IOException ignored) {
        }
        if(usb.serialPort.getSupportedControlLines().contains(UsbSerialPort.ControlLine.RI) ) {
            try {
                usb.serialPort.getRI();
                if(!(usb.serialDriver instanceof ProlificSerialDriver))
                    fail("getRI error expected");
            } catch (IOException ignored) {
            }
        }
        if(purged) {
            try {
                usb.serialPort.purgeHwBuffers(true, true);
                fail("setRts error expected");
            } catch (IOException ignored) {
            }
        }
        usb.close();
        try {
            usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD, UsbWrapper.OpenCloseFlags.NO_DEVICE_CONNECTION));
            fail("open error expected");
        } catch (IOException ignored) {
        }

        usb.open(EnumSet.of(UsbWrapper.OpenCloseFlags.NO_IOMANAGER_THREAD));
        usb.write("x".getBytes());
        UsbDeviceConnection otherDeviceConnection = usbManager.openDevice(usb.serialDriver.getDevice());
        usb.write("x".getBytes());
        otherDeviceConnection.close();
        usb.write("x".getBytes());

        // already queued read request is not interrupted by closing deviceConnection and test would hang
    }

    @Test
    public void commonMethods() throws Exception {
        String s;
        assertNotNull(usb.serialPort.getDriver());
        assertNotNull(usb.serialPort.getDevice());
        assertEquals(test_device_port, usb.serialPort.getPortNumber());
        s = usb.serialDriver.toString();
        assertNotEquals(0, s.length());

        assertFalse(usb.serialPort.isOpen());
        usb.open();
        assertTrue(usb.serialPort.isOpen());

        s = usb.serialPort.getSerial();
        // with target sdk 29 can throw SecurityException before USB permission dialog is confirmed
        // not all devices implement serial numbers. some observed values are:
        // FT232         00000000, FTGH4NTX, ...
        // FT2232        <null>
        // CP2102        0001
        // CP2105        0035E46E
        // CH340         <null>
        // PL2303        <null>
        // CDC:Microbit  9900000037024e450034200b0000004a0000000097969901
        // CDC:Digispark <null>

        try {
            usb.open();
            fail("already open error expected");
        } catch (IOException ignored) {
        }
        try {
            usb.ioManager.run();
            fail("already running error expected");
        } catch (IllegalStateException ignored) {
        }
    }

    @Test
    public void ftdiMethods() throws Exception {
        if(!(usb.serialDriver instanceof FtdiSerialDriver))
            return;
        byte[] b;
        usb.open();
        usb.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);
        telnet.setParameters(115200, 8, 1, UsbSerialPort.PARITY_NONE);

        FtdiSerialDriver.FtdiSerialPort ftdiSerialPort = (FtdiSerialDriver.FtdiSerialPort) usb.serialPort;
        int lt = ftdiSerialPort.getLatencyTimer();
        ftdiSerialPort.setLatencyTimer(1);
        telnet.write("x".getBytes());
        b = usb.read(1);
        long t1 = System.currentTimeMillis();
        telnet.write("x".getBytes());
        b = usb.read(1);
        ftdiSerialPort.setLatencyTimer(100);
        long t2 = System.currentTimeMillis();
        telnet.write("x".getBytes());
        b = usb.read(1);
        long t3 = System.currentTimeMillis();
        ftdiSerialPort.setLatencyTimer(lt);
        assertEquals("latency 1", 99, Math.max(t2-t1, 99)); // looks strange, but shows actual value
        assertEquals("latency 100", 99, Math.min(t3-t2, 99));
    }
}
