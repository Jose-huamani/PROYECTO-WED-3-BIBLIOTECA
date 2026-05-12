package com.example.movilwed3.presentation.books

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Book
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavController
import coil.compose.AsyncImage
import com.example.movilwed3.data.remote.dto.LibroDto
import com.example.movilwed3.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun BooksScreen(
    navController: NavController,
    viewModel: BooksViewModel = hiltViewModel()
) {
    val isLoading by viewModel.isLoading
    val searchQuery by viewModel.searchQuery
    val libros = viewModel.filteredLibros

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
    ) {
        // App Bar / Search
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
                    text = "Explorar",
                    fontSize = 28.sp,
                    fontWeight = FontWeight.Bold,
                    color = WhitePure
                )
                Spacer(modifier = Modifier.height(16.dp))
                
                OutlinedTextField(
                    value = searchQuery,
                    onValueChange = viewModel::onSearchQueryChange,
                    modifier = Modifier.fillMaxWidth(),
                    placeholder = { Text("Buscar por título, autor...", color = GrayLight) },
                    leadingIcon = { Icon(Icons.Default.Search, contentDescription = null, tint = CyanAccent) },
                    colors = TextFieldDefaults.colors(
                        focusedIndicatorColor = CyanAccent,
                        unfocusedIndicatorColor = GrayDark,
                        focusedContainerColor = NavyLight,
                        unfocusedContainerColor = NavyLight,
                        focusedTextColor = WhitePure,
                        unfocusedTextColor = WhitePure
                    ),
                    singleLine = true,
                    shape = RoundedCornerShape(16.dp)
                )
            }
        }

        if (isLoading) {
            Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                CircularProgressIndicator(color = CyanAccent)
            }
        } else {
            LazyVerticalGrid(
                columns = GridCells.Fixed(2),
                contentPadding = PaddingValues(16.dp),
                horizontalArrangement = Arrangement.spacedBy(16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp),
                modifier = Modifier.fillMaxSize()
            ) {
                items(libros) { libro ->
                    BookCard(libro = libro, onClick = {
                        navController.navigate("book_detail/${libro.id}")
                    })
                }
            }
        }
    }
}

@Composable
fun BookCard(libro: LibroDto, onClick: () -> Unit) {
    Card(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(280.dp),
        colors = CardDefaults.cardColors(containerColor = NavyLight),
        shape = RoundedCornerShape(16.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
    ) {
        Column(
            modifier = Modifier.fillMaxSize()
        ) {
            // Image
            val imageUrl = if (libro.imagenUrl.isNullOrEmpty()) {
                "https://via.placeholder.com/300x400.png?text=Sin+Portada"
            } else if (libro.imagenUrl.startsWith("http")) {
                libro.imagenUrl
            } else {
                val path = if (libro.imagenUrl.startsWith("/")) libro.imagenUrl else "/${libro.imagenUrl}"
                "https://10.0.2.2:7223$path"
            }
            
            coil.compose.SubcomposeAsyncImage(
                model = coil.request.ImageRequest.Builder(androidx.compose.ui.platform.LocalContext.current)
                    .data(imageUrl)
                    .crossfade(true)
                    .build(),
                contentDescription = libro.titulo,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(180.dp)
                    .clip(RoundedCornerShape(topStart = 16.dp, topEnd = 16.dp)),
                contentScale = ContentScale.Crop,
                loading = {
                    Box(modifier = Modifier.fillMaxSize().background(NavyLight), contentAlignment = Alignment.Center) {
                        CircularProgressIndicator(color = CyanAccent)
                    }
                },
                error = {
                    Box(modifier = Modifier.fillMaxSize().background(NavyLight), contentAlignment = Alignment.Center) {
                        Column(horizontalAlignment = Alignment.CenterHorizontally) {
                            Icon(Icons.Filled.Book, contentDescription = null, tint = GrayLight, modifier = Modifier.size(32.dp))
                            Spacer(modifier = Modifier.height(4.dp))
                            Text("Sin Portada", color = GrayLight, fontSize = 10.sp, fontWeight = FontWeight.Bold)
                        }
                    }
                }
            )
            
            // Details
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(12.dp),
                verticalArrangement = Arrangement.SpaceBetween
            ) {
                Text(
                    text = libro.titulo,
                    color = WhitePure,
                    fontSize = 14.sp,
                    fontWeight = FontWeight.Bold,
                    maxLines = 2,
                    overflow = TextOverflow.Ellipsis
                )
                
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text(
                        text = libro.autor?.nombre ?: "Anónimo",
                        color = GrayLight,
                        fontSize = 12.sp,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        modifier = Modifier.weight(1f)
                    )
                    Text(
                        text = "${libro.precio} Bs",
                        color = CyanAccent,
                        fontSize = 14.sp,
                        fontWeight = FontWeight.Black
                    )
                }
            }
        }
    }
}
