package com.example.harbor_app;

import android.content.Intent;
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

import org.json.JSONException;
import org.json.JSONObject;

import java.net.URISyntaxException;

public class MainDrive extends AppCompatActivity {
    private Socket socket;
    TextView spdText;
    TextView angText;
    Button stpBtn;
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.drive_main);
        stpBtn=(Button)findViewById(R.id.stopBtn);
        try {
            socket = IO.socket("http://10.210.24.23:8080/");
            socket.connect();
            socket.on(Socket.EVENT_CONNECT, onConnect);
            socket.on("control stream", controlData);

            stpBtn.setOnClickListener(new View.OnClickListener() {
                public void onClick(View view) {
                    socket.disconnect();
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

    private Emitter.Listener controlData = new Emitter.Listener() {
        @Override
        public void call(Object... args) {
            try {
                JSONObject receivedData = (JSONObject) args[0];
                Log.d("Server/speed", receivedData.getString("speed"));
                Log.d("Server/angle", receivedData.getString("angle"));
                spdText = (TextView) findViewById(R.id.getSpeed);
                angText = (TextView) findViewById(R.id.getAngle);
                spdText.setText(receivedData.getString("speed"));
                angText.setText(receivedData.getString("angle"));
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }


    };
}
