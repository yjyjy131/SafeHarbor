package com.example.harbor_app;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;

import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

    }

    public void jbtn(View view) {
        Intent intent = new Intent(getApplicationContext(), MainGPS.class);
        startActivity(intent);
    }

    public void gbtn(View view) {
        Intent intent = new Intent(getApplicationContext(), MainGPS.class);
        startActivity(intent);
    }


}