package com.example.movilwed3.presentation.books

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.LibroDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class BooksViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _libros = mutableStateOf<List<LibroDto>>(emptyList())
    val libros: State<List<LibroDto>> = _libros

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    private val _searchQuery = mutableStateOf("")
    val searchQuery: State<String> = _searchQuery

    init {
        loadBooks()
    }

    fun loadBooks() {
        viewModelScope.launch {
            _isLoading.value = true
            val result = repository.getLibros()
            result.onSuccess {
                _libros.value = it
            }
            _isLoading.value = false
        }
    }

    fun onSearchQueryChange(query: String) {
        _searchQuery.value = query
    }

    val filteredLibros: List<LibroDto>
        get() {
            val query = _searchQuery.value.lowercase()
            if (query.isEmpty()) return _libros.value
            
            return _libros.value.filter {
                it.titulo.lowercase().contains(query) ||
                it.autor?.nombre?.lowercase()?.contains(query) == true ||
                it.categoria?.nombre?.lowercase()?.contains(query) == true
            }
        }
}
