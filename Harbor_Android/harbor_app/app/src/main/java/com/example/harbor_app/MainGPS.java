package com.example.harbor_app;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

public class MainGPS extends AppCompatActivity {

    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.gps_main);
    }

    // 0:북쪽, 1:서쪽, 2:동쪽, 3:남쪽
    public void north(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 0);
        startActivity(intent);
    }

    public void west(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 1);
        startActivity(intent);
    }

    public void east(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 2);
        startActivity(intent);
    }

    public void south(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 3);
        startActivity(intent);
    }
    public void center(View view) {
        Intent intent = new Intent(getApplicationContext(), submit_GPS.class);
        intent.putExtra("dir_phone", 4);
        startActivity(intent);
    }
}
