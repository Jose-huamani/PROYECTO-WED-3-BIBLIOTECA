package com.example.movilwed3

import android.app.Application
import android.util.Log
import dagger.hilt.android.HiltAndroidApp

@HiltAndroidApp
class LibraryApp : Application() {

    override fun onCreate() {
        super.onCreate()

        initApp()
    }

    private fun initApp() {

        Log.d(
            "LibraryApp",
            "Aplicación iniciada correctamente"
        )

        // Aquí luego podrás inicializar:
        //
        // Firebase
        // SignalR
        // Timber
        // Room Database
        // WorkManager
        // Notifications
        // SharedPreferences
    }
}