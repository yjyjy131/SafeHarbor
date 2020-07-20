package com.example.harbor_app;

import android.content.Context;
import android.content.Intent;
import android.location.LocationManager;
import android.os.Bundle;
import android.util.Log;
import android.widget.Button;

import androidx.appcompat.app.AppCompatActivity;

import com.github.nkzawa.emitter.Emitter;
import com.github.nkzawa.socketio.client.IO;
import com.github.nkzawa.socketio.client.Socket;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;
import java.util.Timer;
import java.util.TimerTask;


public class submit_GPS extends AppCompatActivity {
    private String TAG = getString(R.string.received);
    private Socket mSocket;
    Button btn;
    Intent intent;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.gps_submit);

        final LocationManager lm = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        btn=(Button)findViewById(R.id.stopbtn);

        try {
            mSocket = IO.socket("http://192.168.37.108:8080");
            mSocket.connect();
            TimerTask tt = new TimerTask() {
                //TimerTask 추상클래스를 선언하자마자 run()을 강제로 정의하도록 한다.
                @Override
                public void run() {
                    /////////////////// 추가한 코드 ////////////////////
                    JSONObject jsonObject = new JSONObject();
                    try {
                        jsonObject.put("gpsX", "135");
                        jsonObject.put("gpsY", "124");
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                    mSocket.emit("operator gps stream", jsonObject);
                    //////////////////////////////////////////////////
                }

            };
            Timer timer= new Timer();
            timer.schedule(tt,0,5000);
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
                Log.d(TAG, receivedData.getString("server"));
                Log.d(TAG, receivedData.getString("data"));
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    };
}
