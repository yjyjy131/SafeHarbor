package com.example.harbor_app;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbDeviceConnection;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.felhr.usbserial.UsbSerialDevice;
import com.github.nkzawa.emitter.Emitter;
import com.github.nkzawa.socketio.client.IO;
import com.github.nkzawa.socketio.client.Socket;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;


public class submit_GPS extends AppCompatActivity implements LocationListener, SensorEventListener {

    private Socket mSocket;
    Button btn;
    Intent intent;
    TextView dateNow;
    TextView checkText;
    int dir_phone;

    //gps센서

    LocationManager locationManager;
    String gpsX;
    String gpsY;
    List<String> listProviders;
    TextView gpsLatitude, gpsLongitude;
    Location lastKnownLocation;
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
    String rotate;

    boolean isConnected=false;
    TextView connect;

    @SuppressLint("SetTextI18n")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.gps_submit);
        btn = findViewById(R.id.stopbtn);
        intent = getIntent();
        dir_phone = intent.getIntExtra("dir_phone", 0);
        dateNow = findViewById(R.id.dateNow);
        gpsLatitude = findViewById(R.id.gpsLa);
        gpsLongitude = findViewById(R.id.gpsLo);
        checkText = findViewById(R.id.check);

        //가속도센서 on
        mSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        mAccelerometer = mSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        mMagnetometer = mSensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD);


        // TextView 에 현재 시간 문자열 할당
        //권한 체크
        int permissionCheck = ContextCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION);
        if (permissionCheck == PackageManager.PERMISSION_GRANTED) {
            Toast.makeText(getApplicationContext(), "GPS 권한 있음", Toast.LENGTH_SHORT).show();
        }
        locationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        lastKnownLocation = locationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
        if (lastKnownLocation != null) {
            double lng = lastKnownLocation.getLongitude();
            double lat = lastKnownLocation.getLatitude();

            gpsX = String.valueOf(lat);
            gpsY = String.valueOf(lng);
            //gpsLatitude.setText(gpsX);
            //gpsLongitude.setText(gpsY);
        }
        listProviders = locationManager.getAllProviders();
        if (listProviders.get(0).equals(LocationManager.GPS_PROVIDER)) {
            locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 5000, 100, this);
        }
        try {

            mSocket = IO.socket("http://221.167.229.34:8080/");

            mSocket.connect();
            TimerTask tt = new TimerTask() {
                //TimerTask 추상클래스를 선언하자마자 run()을 강제로 정의하도록 한다.
                @SuppressLint({"SimpleDateFormat", "SetTextI18n"})
                @Override
                public void run() {
                    String loc;
                    Date date = new Date();
                    // 시간을 나타냇 포맷
                    SimpleDateFormat sdfNow;
                    sdfNow = new SimpleDateFormat(getString(R.string.timeformat));
                    // String 변수에 값 저장
                    String formatDate = sdfNow.format(date);
                    dateNow.setText(getString(R.string.nowtime) + "\n" + formatDate);
                    /////////////////// 서버로 보내는 값 : gpsX, gpsY, location, time ////////////////////
                    JSONObject jsonObject = new JSONObject();
                    try {
                        switch (dir_phone) {
                            case 0:
                                loc = "front";
                                break;
                            case 1:
                                loc = "left";
                                break;
                            case 2:
                                loc = "right";
                                break;
                            case 3:
                                loc = "back";
                                break;
                            case 4:
                                loc = "center";
                                break;
                            default:
                                loc = "error";
                                break;
                        }
                        jsonObject.put("ClientType", "opd");
                        jsonObject.put("gpsX", gpsX);
                        jsonObject.put("gpsY", gpsY);
                        jsonObject.put("location", loc);
                        jsonObject.put("rotation", rotate);
                        jsonObject.put("time", date);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    mSocket.emit("operator gps stream", jsonObject);
                    //////////////////////////////////////////////////
                }

            };
            final Timer timer = new Timer();
            timer.schedule(tt, 1500, 4000);
            btn.setOnClickListener(new View.OnClickListener() {
                public void onClick(View view) {
                    mSocket.disconnect();
                    timer.cancel();
                    Intent goIntent = new Intent(getApplicationContext(), MainActivity.class);
                    startActivity(goIntent);
                }
            });

            //mSocket.on(Socket.EVENT_CONNECT, onConnect);
            mSocket.on("serverMessage", onMessageReceived);
        } catch (URISyntaxException e) {
            e.printStackTrace();
        }
    }

    // Socket connect 되자마자 발생하는 이벤트
    /*private Emitter.Listener onConnect = new Emitter.Listener() {
        @Override
        public void call(Object... args) {
            JSONObject jsonObject = new JSONObject();
            try {
                jsonObject.put("gpsX", "135");
                jsonObject.put("gpsY", "124");
            } catch (JSONException e) {
                e.printStackTrace();
            }
            mSocket.emit("operator gps stream", jsonObject);
        }
    };
*/
    // 서버에서 받은 message 로그로 출력
    private Emitter.Listener onMessageReceived = new Emitter.Listener() {
        @Override
        public void call(Object... args) {
            // 전달 받은 var message 값 추출하기
            try {
                JSONObject receivedData = (JSONObject) args[0];
                Log.d("Server", receivedData.getString("server"));
                Log.d("Server", receivedData.getString("data"));
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    };

    /////////////////////////////////////////////////////gps함수/////////////////////////////////////
    //gps권한체크용~~//

    //~~gps권한체크용


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
    //gps리스너

    @SuppressLint("SetTextI18n")
    public void onLocationChanged(Location location) {

        double latitude;
        double longitude;

        if (location.getProvider().equals(LocationManager.GPS_PROVIDER)) {
            latitude = location.getLatitude();
            longitude = location.getLongitude();
            Log.d("GPS", latitude + '/' + Double.toString(longitude));
            gpsX = String.valueOf(latitude);
            gpsY = String.valueOf(longitude);

            gpsLatitude.setText(gpsX);
            gpsLongitude.setText(gpsY);

            connect=findViewById(R.id.connect);
            if(mSocket.connected())
            {
                connect.setText("True");
            }
            else
            {
                connect.setText("False");
            }
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
            rotate = String.valueOf(mCurrentDegree);
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {

    }

    @Override
    public void onStatusChanged(String provider, int status, Bundle extras) {
    }

}

