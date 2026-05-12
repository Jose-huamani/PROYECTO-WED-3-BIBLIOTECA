package com.example.movilwed3.presentation.favorites

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.FavoritoDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class FavoritesViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _favoritos = mutableStateOf<List<FavoritoDto>>(emptyList())
    val favoritos: State<List<FavoritoDto>> = _favoritos

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    private val _message = mutableStateOf<String?>(null)
    val message: State<String?> = _message

    init {
        loadFavoritos()
    }

    private fun loadFavoritos() {
        viewModelScope.launch {
            _isLoading.value = true
            val result = repository.getFavoritos()
            result.onSuccess {
                _favoritos.value = it
            }.onFailure {
                _message.value = "Error al cargar favoritos: ${it.message}"
            }
            _isLoading.value = false
        }
    }

    fun clearMessage() {
        _message.value = null
    }
}
