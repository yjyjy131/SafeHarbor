package com.example.harbor_app;

import android.annotation.SuppressLint;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
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
import android.os.IBinder;
import android.os.Message;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

import com.github.nkzawa.emitter.Emitter;
import com.github.nkzawa.socketio.client.IO;
import com.github.nkzawa.socketio.client.Socket;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import org.w3c.dom.Text;

import java.lang.ref.WeakReference;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.List;
import java.util.Set;
import java.util.Timer;
import java.util.TimerTask;
import com.google.gson.Gson;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

public class MainDrive extends AppCompatActivity implements LocationListener, SensorEventListener {

    private Socket socket;
    Button stpBtn;
    TextView urladdress;
    TextView connect;
    SensorManager sensorManager;
    String userId;
    TextView idView;
    String gear;
    String getAngle;
    //시리얼통신
    private UsbService usbService;
    private MyHandler mHandler;
    EditText editText;
    private final ServiceConnection usbConnection = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName arg0, IBinder arg1) {
            usbService = ((UsbService.UsbBinder) arg1).getService();
            usbService.setHandler(mHandler);
            //TextView serial=(TextView)findViewById(R.id.serial);
           // serial.setText(Boolean.toString(usbService.isOpen()));
        }

        @Override
        public void onServiceDisconnected(ComponentName arg0) {
            usbService = null;
        }
    };
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

    @SuppressLint("SimpleDateFormat")
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.drive_main);
        Intent intent = getIntent();
        String url = intent.getStringExtra("url");
        userId = intent.getStringExtra("userId");
        urladdress = findViewById(R.id.urladdress);
        urladdress.setText(url);
        stpBtn = (Button) findViewById(R.id.stopBtn);
        connect = findViewById(R.id.connect);
        idView=(TextView)findViewById(R.id.userId);
        idView.setText(userId);
        //시리얼통신

        mHandler = new MyHandler(this);
        Button sendButton = (Button) findViewById(R.id.buttonSend);
        editText= (EditText)findViewById(R.id.editText1);
        sendButton.setOnClickListener(click);

        //가속도센서 on
        sensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        mSensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);
        mAccelerometer = mSensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        mMagnetometer = mSensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD);
        speed = findViewById(R.id.getSpeed);
        angle = findViewById(R.id.getAngle);
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

            gpsX = String.valueOf(lat);
            gpsY = String.valueOf(lng);
        }
        listProviders = locationManager.getAllProviders();
        if (listProviders.get(0).equals(LocationManager.GPS_PROVIDER)) {
            locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 100, this);
        }
        //소켓 connect
        try {
            socket = IO.socket(url);
            //socket = IO.socket("http://ksyksy12.iptime.org:33337/");
            socket.connect();
            //////////////////////////////////////////////////
            socket.on(Socket.EVENT_CONNECT, onConnect);
            socket.on("control stream", controlData); //speed, angle, time 수신
            TimerTask tt = new TimerTask() {
                @SuppressLint("SetTextI18n")
                @Override
                public void run() {
                    sendDrone();
                    Message lMsg = lHandler.obtainMessage();
                    lHandler.sendMessage(lMsg);
                    if (socket.connected()) {
                        Message msg = oHandler.obtainMessage();
                        oHandler.sendMessage(msg);
                    } else {
                        Message msg = cHandler.obtainMessage();
                        cHandler.sendMessage(msg);
                    }

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

    Handler oHandler = new Handler(new Handler.Callback() {
        @SuppressLint("SetTextI18n")
        @Override
        public boolean handleMessage(Message msg) {
            // todo
            connect.setText("Socket is open");
            return true;
        }
    });
    Handler cHandler = new Handler(new Handler.Callback() {
        @SuppressLint("SetTextI18n")
        @Override
        public boolean handleMessage(Message msg) {
            // todo
            connect.setText("Socket is close");
            return true;
        }
    });
    private Emitter.Listener onConnect = new Emitter.Listener() {
        @Override
        public void call(Object... args) {
            JSONObject clientInfo = new JSONObject();
            try {
                clientInfo.put("clientType", "ctd");
                clientInfo.put("userid", userId);
                socket.emit("client connected", clientInfo);
                Log.d("소켓", socket.id());
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
                Gson gson=new Gson();
                String data=gson.toJson(args);
                JSONArray dataArray=new JSONArray(data);
                JSONObject tempObject=dataArray.getJSONObject(0);
                JSONObject receivedData=tempObject.getJSONObject("nameValuePairs");
                gear=receivedData.getString("gear");
                getAngle=receivedData.getString("angle");
                Log.d("Drone", "데이터받음, gear:" + gear + ", angle:" + getAngle);
                Message msg = saHandler.obtainMessage();
                saHandler.sendMessage(msg);
                /*
                send = receivedData.getString("speed");
                send = send.concat(",");
                send = send.concat(receivedData.getString("angle"));
                send = send.concat(".");
                 */
                //usbService.write(send.getBytes());
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }

    };
    Handler saHandler = new Handler(new Handler.Callback() {
        @SuppressLint("SetTextI18n")
        @Override
        public boolean handleMessage(Message msg) {
            // todo
            speed.setText(gear);
            angle.setText(getAngle);
            return true;
        }
    });


    void sendDrone() {
        try {
            Date date = new Date();
            JSONObject droneInfo = new JSONObject();
            droneInfo.put("userid", userId);
            droneInfo.put("gpsX", gpsX);
            droneInfo.put("gpsY", gpsY);
            droneInfo.put("speed", strSpeed);
            droneInfo.put("angle", strAngle);
            droneInfo.put("time", date);
            socket.emit("drone data stream", droneInfo);
            if (socket.connected())
                Log.d("전송값", droneInfo.toString());
            //Log.d("방향", android.sensor.
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    //gps, 가속도 센서//
    protected void onStart() {
        super.onStart();

    }

    @Override

    protected void onPause() {
        super.onPause();
        locationManager.removeUpdates(this);

        mSensorManager.unregisterListener(this, mAccelerometer);
        mSensorManager.unregisterListener(this, mMagnetometer);
//시리얼코드//
        //unregisterReceiver(mUsbReceiver);
        //unbindService(usbConnection);
    }

    @SuppressLint("SetTextI18n")
    @Override
    protected void onResume() {
        super.onResume();
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
            return;
        }
        locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, this);
        mSensorManager.registerListener(this, mAccelerometer, SensorManager.SENSOR_DELAY_GAME);
        mSensorManager.registerListener(this, mMagnetometer, SensorManager.SENSOR_DELAY_GAME);
//시리얼코드//
        setFilters();  // Start listening notifications from UsbService
        startService(UsbService.class, usbConnection, null); // Start UsbService(if it was not started before) and Bind it
    }

//위치 리스너

    @SuppressLint("SetTextI18n")
    public void onLocationChanged(Location location) {

        double latitude;
        double longitude;
        //double deltaTime = (location.getTime() - lastKnownLocation.getTime()) / 1000.0;
        if (location.getProvider().equals(LocationManager.GPS_PROVIDER)) {
            //strSpeed = String.valueOf(Math.round(lastKnownLocation.distanceTo(location) / deltaTime * 100) / 100.0);
            latitude = location.getLatitude();
            longitude = location.getLongitude();
            gpsX = String.valueOf(latitude);
            gpsY = String.valueOf(longitude);

            Message msg = lHandler.obtainMessage();
            lHandler.sendMessage(msg);
            if (socket.connected()) {
                Message oMsg=oHandler.obtainMessage();
                oHandler.sendMessage(oMsg);
            } else {
                Message cMsg=cHandler.obtainMessage();
                cHandler.sendMessage(cMsg);
            }
        }
    }

    Handler lHandler = new Handler(new Handler.Callback() {
        @SuppressLint("SetTextI18n")
        @Override
        public boolean handleMessage(Message msg) {
            // todo
            gpsLa.setText(gpsX);
            gpsLo.setText(gpsY);
            return true;
        }
    });


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
            mCurrentDegree += 360;
            strAngle = String.valueOf(mCurrentDegree);
            nowAngle.setText(strAngle);
        }
    }

    public void onProviderEnabled(String provider) {
        /*
        if (ActivityCompat.checkSelfPermission(this, android.Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED) {
        }
        locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 100, this);
        locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 100, this);
        locationManager.requestLocationUpdates(LocationManager.PASSIVE_PROVIDER, 0, 0, this);
         */
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
                    Toast.makeText(mActivity.get(), "CTS_CHANGE", Toast.LENGTH_LONG).show();
                    break;
                case UsbService.DSR_CHANGE:
                    Toast.makeText(mActivity.get(), "DSR_CHANGE", Toast.LENGTH_LONG).show();
                    break;
            }
        }
    }

    View.OnClickListener click= new View.OnClickListener() {
        @Override
        public void onClick(View v){
            if (!editText.getText().toString().equals("")) {
                String data = editText.getText().toString();
                Log.d("Serial",data);
                if (usbService != null) { // if UsbService was correctly binded, Send data
                    usbService.write(data.getBytes());
                }
            }
        }
    };
}
