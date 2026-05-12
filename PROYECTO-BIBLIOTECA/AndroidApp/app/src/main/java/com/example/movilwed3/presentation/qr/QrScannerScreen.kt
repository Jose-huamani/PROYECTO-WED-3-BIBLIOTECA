package com.example.movilwed3.presentation.qr

import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.QrCodeScanner
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import com.example.movilwed3.ui.theme.CyanAccent
import com.example.movilwed3.ui.theme.GrayLight
import com.example.movilwed3.ui.theme.NavyDark
import com.example.movilwed3.ui.theme.WhitePure
import com.journeyapps.barcodescanner.ScanContract
import com.journeyapps.barcodescanner.ScanOptions

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun QrScannerScreen(navController: NavController) {
    val context = LocalContext.current
    var scanResult by remember { mutableStateOf<String?>(null) }

    val scanLauncher = rememberLauncherForActivityResult(
        contract = ScanContract(),
        onResult = { result ->
            if (result.contents == null) {
                Toast.makeText(context, "Escaneo cancelado", Toast.LENGTH_SHORT).show()
            } else {
                scanResult = result.contents
                // Suponiendo que el QR/Barras contiene el ID del libro directamente 
                // o un código que podamos procesar. Para este ejemplo, asumimos que es el ID numérico.
                val bookId = scanResult?.toIntOrNull()
                if (bookId != null) {
                    navController.navigate("book_detail/$bookId") {
                        popUpTo("qr_scanner_screen") { inclusive = true }
                    }
                } else {
                    Toast.makeText(context, "Código no reconocido: $scanResult", Toast.LENGTH_LONG).show()
                }
            }
        }
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Escáner Inteligente", color = WhitePure, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = NavyDark),
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Volver", tint = WhitePure)
                    }
                }
            )
        },
        containerColor = NavyDark
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Icon(
                Icons.Default.QrCodeScanner,
                contentDescription = null,
                tint = CyanAccent,
                modifier = Modifier.size(120.dp)
            )
            Spacer(modifier = Modifier.height(32.dp))
            Text(
                "Escanea el código del libro",
                color = WhitePure,
                fontSize = 22.sp,
                fontWeight = FontWeight.Bold
            )
            Spacer(modifier = Modifier.height(16.dp))
            Text(
                "Apunta tu cámara al código QR o de barras ubicado en la contraportada del libro para ver su disponibilidad y detalles.",
                color = GrayLight,
                fontSize = 16.sp,
                modifier = Modifier.padding(horizontal = 16.dp),
                textAlign = androidx.compose.ui.text.style.TextAlign.Center
            )
            Spacer(modifier = Modifier.height(48.dp))
            Button(
                onClick = {
                    val options = ScanOptions()
                    options.setDesiredBarcodeFormats(ScanOptions.ALL_CODE_TYPES)
                    options.setPrompt("Escanea el código del libro")
                    options.setBeepEnabled(true)
                    options.setOrientationLocked(false)
                    scanLauncher.launch(options)
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp),
                colors = ButtonDefaults.buttonColors(containerColor = CyanAccent),
                shape = androidx.compose.foundation.shape.RoundedCornerShape(16.dp)
            ) {
                Text("Abrir Cámara", color = NavyDark, fontWeight = FontWeight.Bold, fontSize = 18.sp)
            }
        }
    }
}
