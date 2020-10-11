package com.example.harbor_app;

import android.annotation.SuppressLint;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.content.pm.PackageManager;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
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

import org.json.JSONException;
import org.json.JSONObject;

import java.lang.ref.WeakReference;
import java.net.URISyntaxException;
import java.util.List;
import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import java.util.Date;

public class MainDrive extends AppCompatActivity implements LocationListener, SensorEventListener {

    private Socket socket;
    Button stpBtn;
    //시리얼통신
    /*
    UsbService usbService;
    private MyHandler mHandler;
    private final ServiceConnection usbConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName arg0, IBinder arg1) {
            usbService = ((UsbService.UsbBinder) arg1).getService();
        }

        @Override
        public void onServiceDisconnected(ComponentName arg0) {
            usbService = null;
        }
    };
     */

    //시리얼 권한 체크
    private final BroadcastReceiver mUsbReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getAction()) {
                case UsbService.ACTION_USB_PERMISSION_GRANTED: // USB PERMISSION GRANTED
                    Toast.makeText(context, "USB Ready", Toast.LENGTH_SHORT).show();
                    break;
                case UsbService.ACTION_USB_PERMISSION_NOT_GRANTED: // USB PERMISSION NOT GRANTED
                    Toast.makeText(context, "USB Permission not granted", Toast.LENGTH_SHORT).show();
                    break;
                case UsbService.ACTION_NO_USB: // NO USB CONNECTED
                    Toast.makeText(context, "No USB connected", Toast.LENGTH_SHORT).show();
                    break;
                case UsbService.ACTION_USB_DISCONNECTED: // USB DISCONNECTED
                    Toast.makeText(context, "USB disconnected", Toast.LENGTH_SHORT).show();
                    break;
                case UsbService.ACTION_USB_NOT_SUPPORTED: // USB NOT SUPPORTED
                    Toast.makeText(context, "USB device not supported", Toast.LENGTH_SHORT).show();
                    break;
            }
        }
    };
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
        //Intent intent=getIntent();
        //String url=intent.getStringExtra("url");
        stpBtn = (Button) findViewById(R.id.stopBtn);
        //시리얼통신
        //mHandler = new MyHandler(this);
        //가속도센서 on
        mSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        mAccelerometer = mSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        mMagnetometer = mSensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD);
        speed = findViewById(R.id.getSpeed);
        angle = findViewById(R.id.getAngle);
        nowSpeed = (TextView) findViewById(R.id.getNowSpeed);
        nowSpeed.setText("0");
        nowAngle = (TextView) findViewById(R.id.getNowAngle);
        nowAngle.setText("0");


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
            locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 1000, 100, this);
            locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 1000, 100, this);

        //소켓 connect
        try {
            //socket=IO.socket(url);
            socket = IO.socket("http://ksyksy12.iptime.org:33337/");
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
            timer.schedule(tt, 1500, 2000);
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
            JSONObject clientInfo = new JSONObject();
            try {
                clientInfo.put("clientType","ctd");
                clientInfo.put("userid","test");
                socket.emit("client connected", clientInfo);
                Log.d("소켓",socket.id());
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    };

    // 서버에서 안드로이드로 speed, angle, time 전송
    private Emitter.Listener controlData = new Emitter.Listener() {
        @SuppressLint("DefaultLocale")
        @Override
        public void call(Object... args) {
            // 시리얼 통신 -> 라즈베리파이로 speed, angle 전송
            String send;
            try {
                JSONObject receivedData = new JSONObject((String) args[0]);
                Log.d("Drone", "데이터받음, speed:"+speed+", angle:"+angle);
                speed.setText(receivedData.getString("speed"));
                angle.setText(receivedData.getString("angle"));
                send = receivedData.getString("speed");
                send = send.concat(",");
                send = send.concat(receivedData.getString("angle"));
                send = send.concat(".");
                //usbService.write(send.getBytes());
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    };

    void sendDrone() {
        try {
            Date date=new Date();
            JSONObject droneInfo = new JSONObject();
            droneInfo.put("userid","test");
            droneInfo.put("gpsX", gpsX);
            droneInfo.put("gpsY", gpsY);
            droneInfo.put("speed", strSpeed);
            droneInfo.put("angle", strAngle);
            droneInfo.put("time",date);
            socket.emit("drone data stream", droneInfo);
            Log.d("Drone", "현재상태 전송!, 앵글 값:"+strAngle);
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

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

        unregisterReceiver(mUsbReceiver);
       // unbindService(usbConnection);
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

        setFilters();  // Start listening notifications from UsbService
       // startService(UsbService.class, usbConnection, null); // Start UsbService(if it was not started before) and Bind it
    }

//위치 리스너

    @SuppressLint("SetTextI18n")
    public void onLocationChanged(Location location) {

        double latitude;
        double longitude;
        double deltaTime = (location.getTime() - lastKnownLocation.getTime()) / 1000.0;
        if (location.getProvider().equals(LocationManager.GPS_PROVIDER)) {
            strSpeed = String.valueOf(Math.round(lastKnownLocation.distanceTo(location) / deltaTime * 100) / 100.0);
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
            mCurrentDegree+=360;
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

    //시리얼 메소드/클래스
    private void setFilters() {
        IntentFilter filter = new IntentFilter();
        filter.addAction(UsbService.ACTION_USB_PERMISSION_GRANTED);
        filter.addAction(UsbService.ACTION_NO_USB);
        filter.addAction(UsbService.ACTION_USB_DISCONNECTED);
        filter.addAction(UsbService.ACTION_USB_NOT_SUPPORTED);
        filter.addAction(UsbService.ACTION_USB_PERMISSION_NOT_GRANTED);
        registerReceiver(mUsbReceiver, filter);
    }

    private void startService(Class<?> service, ServiceConnection serviceConnection, Bundle extras) {
        if (!UsbService.SERVICE_CONNECTED) {
            Intent startService = new Intent(this, service);
            if (extras != null && !extras.isEmpty()) {
                Set<String> keys = extras.keySet();
                for (String key : keys) {
                    String extra = extras.getString(key);
                    startService.putExtra(key, extra);
                }
            }
            startService(startService);
        }
        Intent bindingIntent = new Intent(this, service);
        bindService(bindingIntent, serviceConnection, Context.BIND_AUTO_CREATE);
    }
    private static class MyHandler extends Handler {
        private final WeakReference<MainDrive> mActivity;

        public MyHandler(MainDrive activity) {
            mActivity = new WeakReference<>(activity);
        }

        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case UsbService.MESSAGE_FROM_SERIAL_PORT:
                    String data = (String) msg.obj;
                    break;
                case UsbService.CTS_CHANGE:
                    Toast.makeText(mActivity.get(), "CTS_CHANGE",Toast.LENGTH_LONG).show();
                    break;
                case UsbService.DSR_CHANGE:
                    Toast.makeText(mActivity.get(), "DSR_CHANGE",Toast.LENGTH_LONG).show();
                    break;
            }
        }
    }
}
