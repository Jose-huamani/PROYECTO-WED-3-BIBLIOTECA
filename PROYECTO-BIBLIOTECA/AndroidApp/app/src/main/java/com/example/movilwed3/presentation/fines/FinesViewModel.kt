package com.example.movilwed3.presentation.fines

import androidx.compose.runtime.State
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.MultaDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class FinesViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _multas = mutableStateOf<List<MultaDto>>(emptyList())
    val multas: State<List<MultaDto>> = _multas

    private val _isLoading = mutableStateOf(false)
    val isLoading: State<Boolean> = _isLoading

    init {
        loadFines()
    }

    fun loadFines() {
        viewModelScope.launch {
            _isLoading.value = true
            val result = repository.getMisMultas()
            result.onSuccess {
                _multas.value = it
            }
            _isLoading.value = false
        }
    }
}
