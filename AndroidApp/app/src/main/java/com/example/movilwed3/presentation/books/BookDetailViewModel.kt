package com.example.movilwed3.presentation.books

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.LibroDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class BookDetailViewModel @Inject constructor(
    private val repository: LibraryRepository,
    savedStateHandle: SavedStateHandle
) : ViewModel() {

    private val bookId: Int = checkNotNull(savedStateHandle["bookId"])

    private val _libro = mutableStateOf<LibroDto?>(null)
    val libro: State<LibroDto?> = _libro

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading
    
    private val _actionLoading = mutableStateOf(false)
    val actionLoading: State<Boolean> = _actionLoading
    
    private val _message = mutableStateOf<String?>(null)
    val message: State<String?> = _message

    init {
        loadBookDetail()
    }

    private fun loadBookDetail() {
        viewModelScope.launch {
            _isLoading.value = true
            val result = repository.getLibro(bookId)
            result.onSuccess {
                _libro.value = it
            }
            _isLoading.value = false
        }
    }

    fun solicitarPrestamo(dias: Int) {
        viewModelScope.launch {
            _actionLoading.value = true
            val result = repository.solicitarPrestamo(bookId, dias)
            result.onSuccess {
                _message.value = "Préstamo solicitado exitosamente."
                loadBookDetail() // Refrescar stock
            }.onFailure {
                _message.value = "Error: ${it.message}"
            }
            _actionLoading.value = false
        }
    }

    fun addAlCarrito(cantidad: Int) {
        viewModelScope.launch {
            _actionLoading.value = true
            val result = repository.addCarritoItem(bookId, cantidad)
            result.onSuccess {
                _message.value = "Añadido al carrito de compras."
            }.onFailure {
                _message.value = "Error: ${it.message}"
            }
            _actionLoading.value = false
        }
    }

    fun addFavorito() {
        viewModelScope.launch {
            _actionLoading.value = true
            val result = repository.addFavorito(bookId)
            result.onSuccess {
                _message.value = "Añadido a Favoritos (Me gusta) ♥️"
            }.onFailure {
                _message.value = "Error: ${it.message}"
            }
            _actionLoading.value = false
        }
    }
    
    fun clearMessage() {
        _message.value = null
    }
}
