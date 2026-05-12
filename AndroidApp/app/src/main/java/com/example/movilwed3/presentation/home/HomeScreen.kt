package com.example.movilwed3.presentation.home

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Book
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material.icons.filled.Notifications
import androidx.compose.material.icons.filled.QrCodeScanner
import androidx.compose.material.icons.filled.ShoppingCart
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavController
import com.example.movilwed3.presentation.navigation.Screen
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen(
    navController: NavController,
    onNavigateToBooks: () -> Unit,
    viewModel: HomeViewModel = hiltViewModel()
) {
    val prestamos by viewModel.prestamos
    val multas by viewModel.multas
    val isLoading by viewModel.isLoading

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Mi Panel", fontWeight = FontWeight.Bold, color = WhitePure) },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = NavyDark
                ),
                actions = {
                    IconButton(onClick = { navController.navigate(Screen.QrScanner) }) {
                        Icon(Icons.Default.QrCodeScanner, contentDescription = "Escanear", tint = CyanAccent)
                    }
                    IconButton(onClick = { navController.navigate(Screen.Cart) }) {
                        Icon(Icons.Default.ShoppingCart, contentDescription = "Carrito", tint = CyanAccent)
                    }
                    IconButton(onClick = { navController.navigate(Screen.Notifications) }) {
                        Icon(Icons.Default.Notifications, contentDescription = "Notificaciones", tint = CyanAccent)
                    }
                    IconButton(onClick = {
                        viewModel.logout {
                            navController.navigate(Screen.Login) { popUpTo(0) }
                        }
                    }) {
                        Icon(Icons.Default.ExitToApp, contentDescription = "Salir", tint = ErrorRed)
                    }
                }
            )
        },
        containerColor = NavyDark
    ) { padding ->
        if (isLoading) {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator(color = CyanAccent)
            }
        } else {
            LazyColumn(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
                    .padding(horizontal = 16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                item {
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(
                        text = "¡Hola Lector!",
                        fontSize = 28.sp,
                        fontWeight = FontWeight.Bold,
                        color = CyanAccent
                    )
                    Text(
                        text = "Tu resumen bibliotecario",
                        color = GrayLight,
                        fontSize = 16.sp
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                }

                item {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.spacedBy(16.dp)
                    ) {
                        DashboardCard(
                            modifier = Modifier.weight(1f),
                            title = "Préstamos Activos",
                            value = prestamos.size.toString(),
                            icon = Icons.Default.Book,
                            color = CyanAccentDark
                        )
                        DashboardCard(
                            modifier = Modifier.weight(1f),
                            title = "Multas Pendientes",
                            value = "${multas.sumOf { it.monto }} Bs",
                            icon = Icons.Default.Warning,
                            color = ErrorRed
                        )
                    }
                }

                // Temporary navigation buttons for testing the flow
                item {
                    Spacer(modifier = Modifier.height(32.dp))
                    Text("Navegación Rápida", color = WhitePure, fontWeight = FontWeight.Bold)
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Button(
                        onClick = { onNavigateToBooks() },
                        modifier = Modifier.fillMaxWidth().height(50.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = GlassWhite)
                    ) { Text("Explorar Catálogo", color = CyanAccent) }
                    
                    Spacer(modifier = Modifier.height(8.dp))
                    
                    Button(
                        onClick = { navController.navigate(Screen.Fines) },
                        modifier = Modifier.fillMaxWidth().height(50.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = GlassWhite)
                    ) { Text("Pagar Multas (QR)", color = ErrorRed) }
                }
            }
        }
    }
}

@Composable
fun DashboardCard(
    modifier: Modifier = Modifier,
    title: String,
    value: String,
    icon: ImageVector,
    color: Color
) {
    Card(
        modifier = modifier.height(120.dp),
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(16.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp),
            verticalArrangement = Arrangement.SpaceBetween
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(text = title, color = GrayLight, fontSize = 12.sp, fontWeight = FontWeight.Bold)
                Icon(icon, contentDescription = null, tint = color, modifier = Modifier.size(20.dp))
            }
            Text(
                text = value,
                color = WhitePure,
                fontSize = 28.sp,
                fontWeight = FontWeight.Black
            )
        }
    }
}
