package com.example.harbor_app;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

public class MainGPS extends AppCompatActivity {
    String url;
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.gps_main);
        Intent intent=getIntent();
        url=intent.getStringExtra("url");
    }

    // 0:북쪽, 1:서쪽, 2:동쪽, 3:남쪽
    public void north(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 0);
        intent.putExtra("url",url);
        startActivity(intent);
    }

    public void west(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 1);
        intent.putExtra("url",url);
        startActivity(intent);
    }

    public void east(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 2);
        intent.putExtra("url",url);
        startActivity(intent);
    }

    public void south(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 3);
        intent.putExtra("url",url);
        startActivity(intent);
    }
    public void center(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 4);
        intent.putExtra("url",url);
        startActivity(intent);
    }
}
