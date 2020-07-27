package com.example.harbor_app;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import com.github.nkzawa.emitter.Emitter;
import com.github.nkzawa.socketio.client.IO;
import com.github.nkzawa.socketio.client.Socket;
import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.net.URISyntaxException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class MainDrive extends AppCompatActivity {
    private Socket socket;
    TextView spdText;
    TextView angText;
    Button stpBtn;
    UsbDeviceConnection connection;
    UsbSerialPort port;
    int numBytesRead;
    String substr, Command;

    @SuppressLint("SimpleDateFormat")
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.drive_main);
        stpBtn = (Button) findViewById(R.id.stopBtn);
        //InitArduino();
        try {
            socket = IO.socket("http://10.210.24.17:8080/");
            socket.connect();
            //////////////////////////////////////////////////
            socket.on(Socket.EVENT_CONNECT, onConnect);
            socket.on("control stream", controlData); //speed, angle, time 수신
            stpBtn.setOnClickListener(new View.OnClickListener() {
                public void onClick(View view) {
                    socket.disconnect();//소켓통신 종료
                    try {
                        port.close(); //시리얼통신 종료
                    } catch (IOException e) {
                        System.out.println("IOException occured");
                    }
                    Intent goIntent = new Intent(getApplicationContext(), MainActivity.class);
                    startActivity(goIntent);

                }
            });
        } catch (URISyntaxException e) {
            e.printStackTrace();
        }

    }

    private Emitter.Listener onConnect = new Emitter.Listener() {
        @Override
        public void call(Object... args) {
            socket.emit("request control stream", 0);
        }
    };
    // 서버에서 안드로 speed, angle, time 전송
    private Emitter.Listener controlData = new Emitter.Listener() {
        @Override
        public void call(Object... args) {
            // 시리얼 통신으로 라즈베리파이로 speed, angle, time 전송
            String send;
            JSONObject receivedData = (JSONObject) args[0];
            byte[] sendByte;
            try {
                send = receivedData.getString("speed");
                send = send.concat(",");
                send = send.concat(receivedData.getString("angle"));
                send = send.concat(".");
                sendByte = binaryStringToByteArray(send);
                port.write(sendByte, 50);
            } catch (JSONException | IOException e) {
                e.printStackTrace();
            }
        }
    };

    public static byte[] binaryStringToByteArray(String s) {
        int count = s.length() / 8;
        byte[] b = new byte[count];
        for (int i = 1; i < count; ++i) {
            String t = s.substring((i - 1) * 8, i * 8);
            b[i - 1] = binaryStringToByte(t);
        }
        return b;
    }

    public static byte binaryStringToByte(String s) {
        byte ret = 0, total = 0;
        for (int i = 0; i < 8; ++i) {
            ret = (s.charAt(7 - i) == '1') ? (byte) (1 << i) : 0;
            total = (byte) (ret | total);
        }
        return total;
    }

    public String byteToBinaryString(byte n) {
        StringBuilder sb = new StringBuilder("00000000");
        for (int bit = 0; bit < 8; bit++) {
            if (((n >> bit) & 1) > 0) {
                sb.setCharAt(7 - bit, '1');
            }
        }
        return sb.toString();
    }

    public String byteArrayToBinaryString(byte[] b) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < b.length; ++i) {
            sb.append(byteToBinaryString(b[i]));
        }
        return sb.toString();
    }


    public void ReadBack() {

        TimerTask timerTask = new TimerTask() {
            @Override
            public void run() {
                try {

                    byte[] buffer = new byte[120];

                    numBytesRead = port.read(buffer, 50);

                    String str = byteArrayToBinaryString(buffer);

            /* 시리얼통신으로 라즈베리파이에서 안드로 speed, angle, time 수신하면 소켓 통신으로 값 전송
                    JSONObject receivedData = (JSONObject) args[0];
                JSONObject jsonObject = new JSONObject();
                try {
                    jsonObject.put("ClientType", "ctd");
                    jsonObject.put("gpsX", "135");
                    jsonObject.put("gpsY", "124");
                    jsonObject.put("time", receivedData.getString("time"));
                    jsonObject.put("speed", receivedData.getString("speed"));
                    jsonObject.put("angle", receivedData.getString("angle"));
                } catch (JSONException e) {
                    e.printStackTrace();
                }
                socket.emit("drone data stream", jsonObject);
     */

                } catch (IOException e) {
                    System.out.println("IOException occured.");
                }
            }
        };
        final Timer timer = new Timer();
        timer.schedule(timerTask, 5000, 1000);

    }

    public void CommandWrite() {
        try {


            byte[] comm = Command.getBytes();
            port.write(comm, 100);


        } catch (IOException e) {
            System.out.println("IOException occured.");
        }
    }

    void InitArduino() {

        try {

            UsbManager manager = (UsbManager) getSystemService(Context.USB_SERVICE);
            List<UsbSerialDriver> availableDrivers = UsbSerialProber.getDefaultProber().findAllDrivers(manager);
            if (availableDrivers.isEmpty()) {
                return;
            }

            UsbSerialDriver driver = availableDrivers.get(0);
            connection = manager.openDevice(driver.getDevice());
            if (connection == null) {
                return;
            }


            port = driver.getPorts().get(0);

            port.open(connection);
            port.setParameters(115200, 8, UsbSerialPort.STOPBITS_1, UsbSerialPort.PARITY_NONE);


            port.purgeHwBuffers(true, true);


        } catch (IOException e) {
            System.out.println("IOException occured.");

        }


        TimerTask timerTask = new TimerTask() {
            @Override
            public void run() {

                ReadBack();
            }
        };

        Timer timer = new Timer();
        timer.schedule(timerTask, 0, 250);


        TimerTask timerWriteTask = new TimerTask() {
            @Override
            public void run() {

                try {
                    port.purgeHwBuffers(true, true);
                } catch (IOException e) {
                    System.out.println("IOException occured.");
                }

                CommandWrite();

            }
        };

        Timer timer2 = new Timer();
        timer2.schedule(timerWriteTask, 0, 250);


    }

}
