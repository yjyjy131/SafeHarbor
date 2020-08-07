package com.example.harbor_app;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;

import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.hardware.usb.UsbDeviceConnection;
import android.hardware.usb.UsbManager;
import android.location.Location;
import android.location.LocationListener;

import android.location.LocationManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;

import androidx.core.content.ContextCompat;


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
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;


public class MainDrive extends AppCompatActivity implements LocationListener, SensorEventListener {

    private Socket socket;
    Button stpBtn;
    //시리얼통신
    /*
    TextView spdText;
    TextView angText;
    UsbDeviceConnection connection;
    UsbSerialPort port;
    int numBytesRead;
    String substr, Command;
<<<<<<< HEAD
=======
    */
    //gps센서
    LocationManager locationManager;
    String gpsX;
    String gpsY;
    List<String> listProviders;
    Location lastKnownLocation;
    TextView gpsLa, gpsLo;
    //방향센서
    SensorManager mSensorManager;
    Sensor mAccelerometer;
    Sensor mMagnetometer;
    private float[] mLastAccelerometer = new float[3];
    private float[] mLastMagnetometer = new float[3];
    private boolean mLastAccelerometerSet = false;
    private boolean mLastMagnetometerSet = false;
    private float[] mR = new float[9];
    private float[] mOrientation = new float[3];
    float mCurrentDegree = 0f;
    String strAngle;
    String strSpeed;
    TextView nowAngle;
    TextView angle;
    //속도센서
    TextView speed;
    TextView nowSpeed;

    @SuppressLint("SimpleDateFormat")
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.drive_main);
        stpBtn = (Button) findViewById(R.id.stopBtn);
        //InitArduino();

        //가속도센서 on
        mSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        mAccelerometer = mSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        mMagnetometer = mSensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD);
        speed = findViewById(R.id.getSpeed);
        angle = findViewById(R.id.getAngle);
        nowSpeed = (TextView) findViewById(R.id.getNowSpeed);
        nowSpeed.setText("0");
        nowAngle = (TextView) findViewById(R.id.getNowAngle);


        //gps권한체크
        int permissionCheck = ContextCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION);
        if (permissionCheck == PackageManager.PERMISSION_GRANTED) {
            Toast.makeText(getApplicationContext(), "GPS 권한 있음", Toast.LENGTH_SHORT).show();
        }
        //gps수신

        gpsLa = findViewById(R.id.getGpsX);
        gpsLo = findViewById(R.id.getGpsY);

        locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        lastKnownLocation = locationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
        if (lastKnownLocation != null) {
            double lng = lastKnownLocation.getLongitude();
            double lat = lastKnownLocation.getLatitude();
            gpsX = String.valueOf(Math.round(lat) * 1000 / 1000.0);
            gpsY = String.valueOf(Math.round(lng) * 1000 / 1000.0);
            gpsLa.setText(gpsX);
            gpsLo.setText(gpsY);
        }
        listProviders = locationManager.getAllProviders();
        if (listProviders.get(0).equals(LocationManager.GPS_PROVIDER)) {
            locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 1000, 100, this);
        }

        //소켓 connect
        try {
            socket = IO.socket("http://121.133.157.160:8080/");
            socket.connect();
            //////////////////////////////////////////////////
            socket.on(Socket.EVENT_CONNECT, onConnect);
            socket.on("control stream", controlData); //speed, angle, time 수신
            TimerTask tt = new TimerTask() {
                @Override
                public void run() {
                    sendDrone();
                }
            };
            final Timer timer = new Timer();
            timer.schedule(tt, 1500, 4000);
            stpBtn.setOnClickListener(new View.OnClickListener() {
                public void onClick(View view) {
                    socket.off(Socket.EVENT_CONNECT, onConnect);
                    socket.off("control stream", controlData); //speed, angle, time 수신
                    socket.disconnect();//소켓통신 종료
                    timer.cancel();
                    /*try {
                        port.close(); //시리얼통신 종료
                    } catch (IOException e) {
                        System.out.println("IOException occured");
                    }*/
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

    // 서버에서 안드로이드로 speed, angle, time 전송
    private Emitter.Listener controlData = new Emitter.Listener() {
        @SuppressLint("DefaultLocale")
        @Override
        public void call(Object... args) {
            Log.d("Drone", "데이터받음");
            // 시리얼 통신 -> 라즈베리파이로 speed, angle 전송
            //String send;
            //byte[] sendByte;//

            try {
                JSONObject receivedData = new JSONObject((String) args[0]);
                speed.setText(receivedData.getString("speed"));
                angle.setText(receivedData.getString("angle"));
                //send = receivedData.getString("speed");
                //send = send.concat(",");
                //send = send.concat(receivedData.getString("angle"));
                //send = send.concat(".");
                //sendByte = binaryStringToByteArray(send);
                //port.write(sendByte, 50);
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    };

    void sendDrone() {
        try {
            JSONObject droneInfo = new JSONObject();
            droneInfo.put("ClientType", "ctd");
            droneInfo.put("gpsX", gpsX);
            droneInfo.put("gpsY", gpsY);
            droneInfo.put("speed", strSpeed);
            droneInfo.put("angle", strAngle);
            socket.emit("drone data stream", droneInfo);
            Log.d("Drone", "현재상태 전송!");
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }
/*
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
        byte ret, total = 0;
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
        for (byte value : b) {
            sb.append(byteToBinaryString(value));
        }
        return sb.toString();
    }
*/
    //readBack()
    /*public void ReadBack() {

        TimerTask timerTask = new TimerTask() {
            @Override
            public void run() {
                try {

                    byte[] buffer = new byte[120];

                    numBytesRead = port.read(buffer, 50);

                    String str = byteArrayToBinaryString(buffer);

            시리얼통신으로 라즈베리파이에서 안드로 speed, angle, time 수신하면 소켓 통신으로 값 전송
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


                } catch (IOException e) {
                    System.out.println("IOException occured.");
                }
            }
        };
        final Timer timer = new Timer();
        timer.schedule(timerTask, 5000, 1000);

    }*/
    //commandWrite()
    /*public void CommandWrite() {
        try {


            byte[] comm = Command.getBytes();
            port.write(comm, 100);


        } catch (IOException e) {
            System.out.println("IOException occured.");
        }
    }*/

    //initAduino()
    /*void InitArduino() {

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


    }*/

    //gps, 가속도 센서//
    protected void onStart() {
        super.onStart();
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(this, new String[]{android.Manifest.permission.ACCESS_FINE_LOCATION, android.Manifest.permission.ACCESS_COARSE_LOCATION}, 100);
        }
    }

    @Override

    protected void onPause() {
        super.onPause();
        locationManager.removeUpdates(this);

        mSensorManager.unregisterListener(this, mAccelerometer);
        mSensorManager.unregisterListener(this, mMagnetometer);
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            return;
        }
        locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, this);

        mSensorManager.registerListener(this, mAccelerometer, SensorManager.SENSOR_DELAY_GAME);
        mSensorManager.registerListener(this, mMagnetometer, SensorManager.SENSOR_DELAY_GAME);
    }

//위치 리스너

    @SuppressLint("SetTextI18n")
    public void onLocationChanged(Location location) {

        double latitude;
        double longitude;
        double deltaTime = (location.getTime() - lastKnownLocation.getTime()) / 1000.0;
        if (location.getProvider().equals(LocationManager.GPS_PROVIDER)) {
            strSpeed = String.valueOf(Math.round(lastKnownLocation.distanceTo(location) / deltaTime*100)/100.0);
            latitude = location.getLatitude();
            longitude = location.getLongitude();
            gpsX = String.valueOf(Math.round(latitude * 1000) / 1000.0);
            gpsY = String.valueOf(Math.round(longitude * 1000) / 1000.0);
            gpsLa.setText(gpsX);
            gpsLo.setText(gpsY);
        }
    }

    //가속도 리스너
    @Override
    public void onSensorChanged(SensorEvent event) {
        if (event.sensor == mAccelerometer) {
            System.arraycopy(event.values, 0, mLastAccelerometer, 0, event.values.length);
            mLastAccelerometerSet = true;
        } else if (event.sensor == mMagnetometer) {
            System.arraycopy(event.values, 0, mLastMagnetometer, 0, event.values.length);
            mLastMagnetometerSet = true;
        }
        if (mLastAccelerometerSet && mLastMagnetometerSet) {
            SensorManager.getRotationMatrix(mR, null, mLastAccelerometer, mLastMagnetometer);
            float azimuthinDegress = (int) (Math.toDegrees(SensorManager.getOrientation(mR, mOrientation)[0]) + 360) % 360;
            mCurrentDegree = -azimuthinDegress;
            strAngle = String.valueOf(mCurrentDegree);
            nowSpeed.setText(strSpeed);
            nowAngle.setText(strAngle);
        }
    }

    public void onProviderEnabled(String provider) {
        /*if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
        }
        //locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 100, this);
        //locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 0, this);
        //locationManager.requestLocationUpdates(LocationManager.PASSIVE_PROVIDER, 0, 0, this);*/
    }

    @Override
    public void onProviderDisabled(String provider) {
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    @Override
    public void onStatusChanged(String provider, int status, Bundle extras) {
    }

    ///////////// 하단 바 숨김 ///////////
}
