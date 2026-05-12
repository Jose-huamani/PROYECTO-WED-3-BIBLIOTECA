package com.example.movilwed3.presentation.reservations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.ReservaDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class ReservationsState(
    val isLoading: Boolean = false,
    val reservations: List<ReservaDto> = emptyList(),
    val error: String? = null
)

@HiltViewModel
class ReservationsViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _state = MutableStateFlow(ReservationsState())
    val state: StateFlow<ReservationsState> = _state.asStateFlow()

    init {
        loadReservations()
    }

    private fun loadReservations() {
        viewModelScope.launch {
            _state.value = _state.value.copy(isLoading = true, error = null)
            val result = repository.getMisReservas()
            result.onSuccess { list ->
                _state.value = _state.value.copy(isLoading = false, reservations = list)
            }.onFailure { error ->
                _state.value = _state.value.copy(
                    isLoading = false,
                    error = error.localizedMessage ?: "Error al cargar reservas"
                )
            }
        }
    }
}
