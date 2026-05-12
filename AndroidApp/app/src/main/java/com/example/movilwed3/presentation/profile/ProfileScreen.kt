package com.example.movilwed3.presentation.profile

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavController
import com.example.movilwed3.presentation.navigation.Screen
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(
    navController: NavController,
    viewModel: ProfileViewModel = hiltViewModel()
) {
    val profile by viewModel.profile
    val isLoading by viewModel.isLoading
    val message by viewModel.message
    val snackbarHostState = remember { SnackbarHostState() }

    var isEditing by remember { mutableStateOf(false) }
    var editNombre by remember { mutableStateOf("") }
    var editPassword by remember { mutableStateOf("") }

    LaunchedEffect(message) {
        message?.let {
            snackbarHostState.showSnackbar(it)
            viewModel.clearMessage()
            isEditing = false
        }
    }

    LaunchedEffect(profile) {
        profile?.let {
            editNombre = it.nombreCompleto
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Mi Perfil", color = WhitePure, fontWeight = FontWeight.Bold) },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = NavyDark)
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = NavyDark
    ) { padding ->
        if (isLoading && profile == null) {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator(color = CyanAccent)
            }
        } else {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(padding)
                    .verticalScroll(rememberScrollState())
                    .padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Avatar Placeholder
                Box(
                    modifier = Modifier
                        .size(100.dp)
                        .clip(CircleShape)
                        .background(NavyLight),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(Icons.Default.Person, contentDescription = null, tint = CyanAccent, modifier = Modifier.size(56.dp))
                }

                Spacer(modifier = Modifier.height(16.dp))

                profile?.let { user ->
                    Text(text = user.nombreCompleto, color = WhitePure, fontSize = 22.sp, fontWeight = FontWeight.Bold)
                    Text(text = user.email, color = GrayLight, fontSize = 14.sp)

                    Spacer(modifier = Modifier.height(24.dp))

                    Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceEvenly) {
                        StatCard("Rol", user.rol)
                        StatCard("Puntos", "${user.puntos}", Icons.Default.Star)
                        StatCard("Estado", if (user.activo) "Activo" else "Inactivo")
                    }

                    Spacer(modifier = Modifier.height(32.dp))

                    if (isEditing) {
                        Card(
                            colors = CardDefaults.cardColors(containerColor = NavyLight),
                            shape = RoundedCornerShape(16.dp),
                            modifier = Modifier.fillMaxWidth()
                        ) {
                            Column(modifier = Modifier.padding(16.dp)) {
                                Text("Editar Información", color = WhitePure, fontWeight = FontWeight.Bold)
                                Spacer(modifier = Modifier.height(16.dp))
                                
                                OutlinedTextField(
                                    value = editNombre,
                                    onValueChange = { editNombre = it },
                                    label = { Text("Nombre Completo") },
                                    modifier = Modifier.fillMaxWidth()
                                )
                                Spacer(modifier = Modifier.height(16.dp))
                                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                                    Button(onClick = { isEditing = false }, modifier = Modifier.weight(1f), colors = ButtonDefaults.buttonColors(containerColor = GrayDark)) {
                                        Text("Cancelar")
                                    }
                                    Button(onClick = { viewModel.updateProfile(editNombre, editPassword) }, modifier = Modifier.weight(1f), colors = ButtonDefaults.buttonColors(containerColor = CyanAccent)) {
                                        Text("Guardar", color = NavyDark)
                                    }
                                }
                            }
                        }
                    } else {
                        Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                            ProfileActionItem("Mis Reservas", Icons.Default.Bookmark) {
                                navController.navigate(Screen.Reservations)
                            }
                            ProfileActionItem("Mis Multas y Pagos", Icons.Default.ReceiptLong) {
                                navController.navigate(Screen.Fines)
                            }
                            ProfileActionItem("Ajustes del Sistema", Icons.Default.Settings) {
                                navController.navigate(Screen.Settings)
                            }
                            
                            Spacer(modifier = Modifier.height(12.dp))

                            Button(
                                onClick = { isEditing = true },
                                modifier = Modifier.fillMaxWidth().height(56.dp),
                                colors = ButtonDefaults.buttonColors(containerColor = NavyLight),
                                shape = RoundedCornerShape(16.dp)
                            ) {
                                Icon(Icons.Default.Edit, contentDescription = null, tint = CyanAccent)
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("Editar Perfil", color = WhitePure, fontWeight = FontWeight.Bold)
                            }
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun ProfileActionItem(title: String, icon: ImageVector, onClick: () -> Unit) {
    Card(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth(),
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(16.dp)
    ) {
        Row(modifier = Modifier.padding(16.dp).fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
            Icon(icon, contentDescription = null, tint = CyanAccent)
            Spacer(modifier = Modifier.width(16.dp))
            Text(title, color = WhitePure, fontWeight = FontWeight.Medium, modifier = Modifier.weight(1f))
            Icon(Icons.Default.ChevronRight, contentDescription = null, tint = GrayLight)
        }
    }
}

@Composable
fun StatCard(title: String, value: String, icon: ImageVector? = null) {
    Card(
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(16.dp),
        modifier = Modifier.size(90.dp)
    ) {
        Column(modifier = Modifier.fillMaxSize().padding(4.dp), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
            if (icon != null) Icon(icon, contentDescription = null, tint = CyanAccent, modifier = Modifier.size(20.dp))
            Text(value, color = WhitePure, fontSize = 14.sp, fontWeight = FontWeight.Bold)
            Text(title, color = GrayLight, fontSize = 10.sp)
        }
    }
}
