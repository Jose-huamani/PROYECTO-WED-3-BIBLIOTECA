package com.example.movilwed3.presentation.fines

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.QrCode
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavController
import com.example.movilwed3.data.remote.dto.MultaDto
import com.example.movilwed3.presentation.navigation.Screen
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FinesScreen(
    navController: NavController,
    viewModel: FinesViewModel = hiltViewModel()
) {
    val multas by viewModel.multas
    val isLoading by viewModel.isLoading

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Control Financiero", color = WhitePure, fontWeight = FontWeight.Bold) },
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
        ) {
            if (isLoading) {
                Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                    CircularProgressIndicator(color = CyanAccent)
                }
            } else if (multas.isEmpty()) {
                Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                    Text("No tienes multas registradas.", color = SuccessGreen, fontWeight = FontWeight.Bold)
                }
            } else {
                LazyColumn(
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(multas) { multa ->
                        FineCard(multa) {
                            navController.navigate("${Screen.PaymentQr}/${multa.monto}/${multa.id}")
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun FineCard(multa: MultaDto, onPay: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(
                    Icons.Default.Warning,
                    contentDescription = null,
                    tint = if (multa.pagada) SuccessGreen else ErrorRed,
                    modifier = Modifier.size(32.dp)
                )
                Spacer(modifier = Modifier.width(12.dp))
                Column(modifier = Modifier.weight(1f)) {
                    Text(multa.motivo, color = WhitePure, fontWeight = FontWeight.Bold, fontSize = 16.sp)
                    Text(multa.fechaGeneracion.take(10), color = GrayLight, fontSize = 12.sp)
                }
                Column(horizontalAlignment = Alignment.End) {
                    Text("${multa.monto} Bs", color = if (multa.pagada) SuccessGreen else ErrorRed, fontSize = 18.sp, fontWeight = FontWeight.Black)
                    Text(if (multa.pagada) "PAGADA" else "PENDIENTE", color = if (multa.pagada) SuccessGreen else ErrorRed, fontSize = 10.sp)
                }
            }
            
            if (!multa.pagada) {
                Spacer(modifier = Modifier.height(16.dp))
                Button(
                    onClick = onPay,
                    modifier = Modifier.fillMaxWidth(),
                    colors = ButtonDefaults.buttonColors(containerColor = CyanAccent),
                    shape = RoundedCornerShape(8.dp)
                ) {
                    Icon(Icons.Default.QrCode, contentDescription = null, tint = NavyDark)
                    Spacer(modifier = Modifier.width(8.dp))
                    Text("Pagar con QR", color = NavyDark, fontWeight = FontWeight.Bold)
                }
            }
        }
    }
}
