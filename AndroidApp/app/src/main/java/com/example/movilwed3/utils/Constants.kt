package com.example.movilwed3.utils

object Constants {
    // Si usas un Emulador de Android Studio, usa el puerto HTTPS 7223 para evitar redirecciones 307:
    const val BASE_URL = "https://10.0.2.2:7223/api/"
    
    // Si usas un dispositivo físico (tu celular) conectado por Wi-Fi, comenta la línea de arriba
    // y usa tu dirección IP local con el puerto HTTPS 7223. Ejemplo:
    // const val BASE_URL = "https://10.254.28.217:7223/api/"
}
