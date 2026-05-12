package com.example.movilwed3.presentation.reservations

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.BookmarkBorder
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavController
import com.example.movilwed3.data.remote.dto.ReservaDto
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ReservationsScreen(
    navController: NavController,
    viewModel: ReservationsViewModel = hiltViewModel()
) {
    val state by viewModel.state.collectAsState()

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Mis Reservas", color = WhitePure, fontWeight = FontWeight.Bold) },
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
        Box(modifier = Modifier.fillMaxSize().padding(padding)) {
            if (state.isLoading) {
                CircularProgressIndicator(color = CyanAccent, modifier = Modifier.align(Alignment.Center))
            } else if (state.error != null) {
                Text(state.error!!, color = ErrorRed, modifier = Modifier.align(Alignment.Center))
            } else if (state.reservations.isEmpty()) {
                Column(
                    modifier = Modifier.align(Alignment.Center),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Icon(Icons.Default.BookmarkBorder, contentDescription = null, tint = GrayLight, modifier = Modifier.size(64.dp))
                    Spacer(modifier = Modifier.height(16.dp))
                    Text("No tienes reservas activas", color = GrayLight, fontSize = 18.sp)
                }
            } else {
                LazyColumn(
                    modifier = Modifier.fillMaxSize(),
                    contentPadding = PaddingValues(16.dp),
                    verticalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    items(state.reservations) { reserva ->
                        ReservaCard(reserva)
                    }
                }
            }
        }
    }
}

@Composable
fun ReservaCard(reserva: ReservaDto) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Text(
                reserva.libro?.titulo ?: "Libro Reservado",
                color = WhitePure,
                fontWeight = FontWeight.Bold,
                fontSize = 16.sp
            )
            Spacer(modifier = Modifier.height(4.dp))
            Text(
                reserva.libro?.autor ?: "Autor desconocido",
                color = GrayLight,
                fontSize = 14.sp
            )
            Spacer(modifier = Modifier.height(12.dp))
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Column {
                    Text("Fecha Reserva", color = GrayLight, fontSize = 10.sp)
                    Text(reserva.fechaReserva.take(10), color = CyanAccent, fontWeight = FontWeight.Bold)
                }
                Column(horizontalAlignment = Alignment.End) {
                    Text("Estado", color = GrayLight, fontSize = 10.sp)
                    Text(
                        reserva.estadoNombre,
                        color = if (reserva.estadoNombre.contains("Aceptada", true) || reserva.estadoNombre.contains("Pendiente", true)) CyanAccent else WhitePure,
                        fontWeight = FontWeight.Black
                    )
                }
            }
        }
    }
}
