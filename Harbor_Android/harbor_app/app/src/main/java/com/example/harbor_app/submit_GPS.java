package com.example.harbor_app;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbManager;
import android.location.LocationManager;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;

import com.github.nkzawa.emitter.Emitter;
import com.github.nkzawa.socketio.client.IO;
import com.github.nkzawa.socketio.client.Socket;
import com.hoho.android.usbserial.driver.UsbSerialDriver;
import com.hoho.android.usbserial.driver.UsbSerialPort;
import com.hoho.android.usbserial.driver.UsbSerialProber;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;
import java.sql.Struct;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.Date;
import java.text.SimpleDateFormat;
import android.widget.TextView;

public class submit_GPS extends AppCompatActivity {

    private Socket mSocket;
    Button btn;
    Intent intent;
    long now = System.currentTimeMillis();
    // 현재시간을 date 변수에 저장한다.

    TextView dateNow;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.gps_submit);
        final LocationManager lm = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        btn = (Button) findViewById(R.id.stopbtn);
        intent = getIntent();
        final int dir_phone = intent.getIntExtra("dir_phone", 0);
        String time;
        dateNow = (TextView) findViewById(R.id.dateNow);
        // TextView 에 현재 시간 문자열 할당
        //time = (String) findViewById(R.id.dateNow);
        try {
            mSocket = IO.socket("http://10.210.24.23:8080/");
            mSocket.connect();
            TimerTask tt = new TimerTask() {
                //TimerTask 추상클래스를 선언하자마자 run()을 강제로 정의하도록 한다.
                @SuppressLint({"SimpleDateFormat", "SetTextI18n"})
                @Override
                public void run() {
                    String location;
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
                                location = "front";
                                break;
                            case 1:
                                location = "left";
                                break;
                            case 2:
                                location = "right";
                                break;
                            case 3:
                                location = "back";
                                break;
                            case 4:
                                location = "center";
                                break;
                            default:
                                location = "error";
                                break;
                        }
                        jsonObject.put("ClientType", "opd");
                        jsonObject.put("gpsX", "135");
                        jsonObject.put("gpsY", "124");
                        jsonObject.put("location", location);
                        jsonObject.put("time", formatDate);
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    mSocket.emit("operator gps stream", jsonObject);
                    //////////////////////////////////////////////////
                }

            };
            final Timer timer = new Timer();
            timer.schedule(tt, 0, 5000);
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
    private Emitter.Listener onConnect = new Emitter.Listener() {
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
}
