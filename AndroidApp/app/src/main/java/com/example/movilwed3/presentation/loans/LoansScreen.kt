package com.example.movilwed3.presentation.loans

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Book
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.example.movilwed3.data.remote.dto.LoanDto
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LoansScreen(
    viewModel: LoansViewModel = hiltViewModel()
) {
    val prestamos by viewModel.prestamos
    val isLoading by viewModel.isLoading

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
    ) {
        Surface(
            color = NavyDark,
            shadowElevation = 8.dp
        ) {
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(16.dp)
            ) {
                Text(
                    text = "Mis Préstamos",
                    fontSize = 28.sp,
                    fontWeight = FontWeight.Bold,
                    color = WhitePure
                )
            }
        }

        if (isLoading) {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator(color = CyanAccent)
            }
        } else if (prestamos.isEmpty()) {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                Text("No tienes préstamos registrados.", color = GrayLight)
            }
        } else {
            LazyColumn(
                contentPadding = PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                items(prestamos) { prestamo ->
                    LoanCard(prestamo)
                }
            }
        }
    }
}

@Composable
fun LoanCard(prestamo: LoanDto) {
    val estadoColor = when (prestamo.statusInt) {
        1 -> WarningYellow // Activo
        2 -> SuccessGreen // Devuelto
        3 -> ErrorRed // Vencido
        else -> GrayLight // Otros
    }
    
    val estadoTexto = when (prestamo.statusInt) {
        1 -> "Activo"
        2 -> "Devuelto"
        3 -> "Vencido"
        4 -> "Cancelado"
        else -> "Desconocido"
    }

    Card(
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(12.dp)
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                Icons.Default.Book,
                contentDescription = null,
                tint = CyanAccent,
                modifier = Modifier.size(40.dp)
            )
            
            Spacer(modifier = Modifier.width(16.dp))
            
            Column(modifier = Modifier.weight(1f)) {
                val libroPrincipal = prestamo.detalles?.firstOrNull()?.libro
                Text(
                    text = libroPrincipal?.titulo ?: "Libro desconocido",
                    color = WhitePure,
                    fontWeight = FontWeight.Bold,
                    fontSize = 16.sp
                )
                if ((prestamo.detalles?.size ?: 0) > 1) {
                    Text(
                        text = "+ ${(prestamo.detalles?.size ?: 0) - 1} libros más",
                        color = CyanAccent,
                        fontSize = 12.sp
                    )
                }
                Spacer(modifier = Modifier.height(4.dp))
                Text(
                    text = "Vence: ${prestamo.dueDate?.take(10) ?: "N/A"}",
                    color = GrayLight,
                    fontSize = 12.sp
                )
            }
            
            SuggestionChip(
                onClick = { },
                label = { Text(estadoTexto, color = estadoColor, fontWeight = FontWeight.Bold) },
                colors = SuggestionChipDefaults.suggestionChipColors(containerColor = NavyDark),
                border = null
            )
        }
    }
}
