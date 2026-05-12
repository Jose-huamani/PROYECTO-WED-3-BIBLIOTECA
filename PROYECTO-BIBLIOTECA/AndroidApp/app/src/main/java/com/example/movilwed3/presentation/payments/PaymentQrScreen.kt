package com.example.movilwed3.presentation.payments

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.QrCode2
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PaymentQrScreen(
    navController: NavController,
    monto: Double,
    multaId: Int
) {
    var paymentProcessing by remember { mutableStateOf(false) }
    var paymentDone by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Pago Seguro QR", color = WhitePure, fontWeight = FontWeight.Bold) },
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
            verticalArrangement = Arrangement.spacedBy(24.dp)
        ) {
            if (!paymentDone) {
                Text(
                    "Escanea para pagar",
                    color = WhitePure,
                    fontSize = 24.sp,
                    fontWeight = FontWeight.Bold
                )

                Card(
                    modifier = Modifier.size(280.dp),
                    colors = CardDefaults.cardColors(containerColor = Color.White),
                    shape = RoundedCornerShape(24.dp)
                ) {
                    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                        // Simulación de QR con un icono grande
                        Icon(
                            Icons.Default.QrCode2,
                            contentDescription = null,
                            tint = Color.Black,
                            modifier = Modifier.fillMaxSize(0.8f)
                        )
                    }
                }

                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("Monto a pagar", color = GrayLight, fontSize = 16.sp)
                    Text("$monto Bs", color = CyanAccent, fontSize = 32.sp, fontWeight = FontWeight.Black)
                }

                Card(
                    modifier = Modifier.fillMaxWidth(),
                    colors = CardDefaults.cardColors(containerColor = NavyLight),
                    shape = RoundedCornerShape(16.dp)
                ) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        Text("Instrucciones:", color = WhitePure, fontWeight = FontWeight.Bold)
                        Text(
                            "1. Abre tu app bancaria.\n2. Escanea el código QR de arriba.\n3. Confirma el pago de $monto Bs.\n4. Presiona el botón de confirmar aquí abajo.",
                            color = GrayLight,
                            fontSize = 14.sp
                        )
                    }
                }

                Spacer(modifier = Modifier.weight(1f))

                Button(
                    onClick = { 
                        paymentProcessing = true
                        // Simulación de espera
                    },
                    modifier = Modifier.fillMaxWidth().height(56.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = CyanAccent),
                    shape = RoundedCornerShape(16.dp),
                    enabled = !paymentProcessing
                ) {
                    if (paymentProcessing) {
                        CircularProgressIndicator(color = NavyDark, modifier = Modifier.size(24.dp))
                    } else {
                        Text("Confirmar Pago Realizado", color = NavyDark, fontWeight = FontWeight.Bold)
                    }
                }
                
                if (paymentProcessing) {
                    LaunchedEffect(Unit) {
                        kotlinx.coroutines.delay(2000)
                        paymentProcessing = false
                        paymentDone = true
                    }
                }
            } else {
                // Pantalla de Éxito
                Spacer(modifier = Modifier.height(64.dp))
                Icon(
                    Icons.Default.QrCode2, // Podría ser CheckCircle pero mantengamos el estilo
                    contentDescription = null,
                    tint = CyanAccent,
                    modifier = Modifier.size(120.dp)
                )
                Text("¡Pago Confirmado!", color = WhitePure, fontSize = 28.sp, fontWeight = FontWeight.Black)
                Text("Tu multa ID #$multaId ha sido liquidada exitosamente.", color = GrayLight, textAlign = androidx.compose.ui.text.style.TextAlign.Center)
                
                Spacer(modifier = Modifier.weight(1f))
                
                Button(
                    onClick = { navController.popBackStack() },
                    modifier = Modifier.fillMaxWidth().height(56.dp),
                    colors = ButtonDefaults.buttonColors(containerColor = GlassWhite),
                    shape = RoundedCornerShape(16.dp)
                ) {
                    Text("Volver", color = CyanAccent)
                }
            }
        }
    }
}
