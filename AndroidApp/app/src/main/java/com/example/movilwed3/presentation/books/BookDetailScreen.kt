package com.example.movilwed3.presentation.books

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.Book
import androidx.compose.material.icons.filled.FavoriteBorder
import androidx.compose.material.icons.filled.Share
import androidx.compose.material.icons.filled.ShoppingCart
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavController
import coil.compose.AsyncImage
import com.example.movilwed3.ui.theme.*

import androidx.compose.ui.platform.LocalContext
import android.content.Intent

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun BookDetailScreen(
    navController: NavController,
    viewModel: BookDetailViewModel = hiltViewModel()
) {
    val libro by viewModel.libro
    val isLoading by viewModel.isLoading
    val actionLoading by viewModel.actionLoading
    val message by viewModel.message
    val snackbarHostState = remember { SnackbarHostState() }
    val context = LocalContext.current

    LaunchedEffect(message) {
        message?.let {
            snackbarHostState.showSnackbar(it)
            viewModel.clearMessage()
        }
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Detalle del Libro", color = WhitePure) },
                navigationIcon = {
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(Icons.Default.ArrowBack, contentDescription = "Atrás", tint = WhitePure)
                    }
                },
                actions = {
                    // Share Button
                    IconButton(onClick = {
                        libro?.let { book ->
                            val shareIntent = Intent(Intent.ACTION_SEND).apply {
                                type = "text/plain"
                                putExtra(Intent.EXTRA_SUBJECT, book.titulo)
                                putExtra(Intent.EXTRA_TEXT, "¡Mira este increíble libro: ${book.titulo} por ${book.autor?.nombre} en la Biblioteca Inteligente!")
                            }
                            context.startActivity(Intent.createChooser(shareIntent, "Compartir Libro"))
                        }
                    }) {
                        Icon(Icons.Filled.Share, contentDescription = "Compartir", tint = CyanAccent)
                    }
                    // Favorite Button
                    IconButton(onClick = { viewModel.addFavorito() }) {
                        Icon(Icons.Filled.FavoriteBorder, contentDescription = "Me gusta", tint = ErrorRed)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = NavyDark)
            )
        },
        snackbarHost = { SnackbarHost(snackbarHostState) },
        containerColor = NavyDark
    ) { padding ->
        if (isLoading) {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator(color = CyanAccent)
            }
        } else {
            libro?.let { book ->
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(padding)
                        .verticalScroll(rememberScrollState())
                ) {
                    // Portada
                    val imageUrl = if (book.imagenUrl.isNullOrEmpty()) {
                        "https://via.placeholder.com/400x500.png?text=Sin+Portada"
                    } else if (book.imagenUrl.startsWith("http")) {
                        book.imagenUrl
                    } else {
                        val path = if (book.imagenUrl.startsWith("/")) book.imagenUrl else "/${book.imagenUrl}"
                        "https://10.0.2.2:7223$path"
                    }

                    coil.compose.SubcomposeAsyncImage(
                        model = coil.request.ImageRequest.Builder(LocalContext.current)
                            .data(imageUrl)
                            .crossfade(true)
                            .build(),
                        contentDescription = book.titulo,
                        modifier = Modifier
                            .fillMaxWidth()
                            .height(350.dp)
                            .background(NavyLight),
                        contentScale = ContentScale.Crop,
                        loading = {
                            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                                CircularProgressIndicator(color = CyanAccent)
                            }
                        },
                        error = {
                            Box(modifier = Modifier.fillMaxSize().background(NavyLight), contentAlignment = Alignment.Center) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                    Icon(Icons.Default.Book, contentDescription = null, tint = GrayLight, modifier = Modifier.size(64.dp))
                                    Spacer(modifier = Modifier.height(8.dp))
                                    Text("Sin Portada", color = GrayLight, fontSize = 16.sp, fontWeight = FontWeight.Bold)
                                }
                            }
                        }
                    )

                    // Detalles
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(24.dp)
                    ) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.Top
                        ) {
                            Column(modifier = Modifier.weight(1f)) {
                                Text(
                                    text = book.titulo,
                                    color = WhitePure,
                                    fontSize = 24.sp,
                                    fontWeight = FontWeight.Black,
                                    lineHeight = 32.sp
                                )
                                Spacer(modifier = Modifier.height(4.dp))
                                Text(
                                    text = "Por ${book.autor?.nombre ?: "Anónimo"}",
                                    color = CyanAccent,
                                    fontSize = 16.sp,
                                    fontWeight = FontWeight.Bold
                                )
                            }
                            
                            Text(
                                text = "${book.precio} Bs",
                                color = SuccessGreen,
                                fontSize = 24.sp,
                                fontWeight = FontWeight.Black
                            )
                        }

                        Spacer(modifier = Modifier.height(16.dp))

                        // Chips (Stock, Categoria)
                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            SuggestionChip(
                                onClick = { },
                                label = { Text(book.categoria?.nombre ?: "Sin Categoría", color = WhitePure) },
                                colors = SuggestionChipDefaults.suggestionChipColors(containerColor = NavyLight),
                                border = null
                            )
                            SuggestionChip(
                                onClick = { },
                                label = { Text("Stock: ${book.cantidadDisponible}", color = if(book.cantidadDisponible > 0) SuccessGreen else ErrorRed) },
                                colors = SuggestionChipDefaults.suggestionChipColors(containerColor = NavyLight),
                                border = null
                            )
                        }

                        Spacer(modifier = Modifier.height(24.dp))

                        Text(
                            text = "Descripción",
                            color = WhitePure,
                            fontSize = 18.sp,
                            fontWeight = FontWeight.Bold
                        )
                        Spacer(modifier = Modifier.height(8.dp))
                        Text(
                            text = book.descripcion ?: book.introduccion ?: "No hay descripción disponible.",
                            color = GrayLight,
                            fontSize = 14.sp,
                            lineHeight = 22.sp
                        )

                        Spacer(modifier = Modifier.height(32.dp))

                        // Botones de Acción
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.spacedBy(16.dp)
                        ) {
                            Button(
                                onClick = { viewModel.solicitarPrestamo(7) }, // 7 dias por defecto
                                modifier = Modifier.weight(1f).height(56.dp),
                                colors = ButtonDefaults.buttonColors(containerColor = NavyLight),
                                shape = RoundedCornerShape(16.dp),
                                enabled = !actionLoading && book.cantidadDisponible > 0
                            ) {
                                Text("Préstamo (7 d)", color = WhitePure, fontWeight = FontWeight.Bold)
                            }

                            Button(
                                onClick = { viewModel.addAlCarrito(1) },
                                modifier = Modifier.weight(1f).height(56.dp),
                                colors = ButtonDefaults.buttonColors(containerColor = CyanAccent),
                                shape = RoundedCornerShape(16.dp),
                                enabled = !actionLoading && book.cantidadDisponible > 0
                            ) {
                                if (actionLoading) {
                                    CircularProgressIndicator(color = NavyDark, modifier = Modifier.size(24.dp))
                                } else {
                                    Icon(Icons.Default.ShoppingCart, contentDescription = null, tint = NavyDark)
                                    Spacer(modifier = Modifier.width(8.dp))
                                    Text("Comprar", color = NavyDark, fontWeight = FontWeight.Bold)
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
