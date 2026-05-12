package com.example.movilwed3.presentation.store

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.movilwed3.data.remote.dto.CarritoDto
import com.example.movilwed3.domain.repository.LibraryRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class CartState(
    val isLoading: Boolean = false,
    val cart: CarritoDto? = null,
    val error: String? = null,
    val checkoutSuccess: Boolean = false
)

@HiltViewModel
class CartViewModel @Inject constructor(
    private val repository: LibraryRepository
) : ViewModel() {

    private val _state = MutableStateFlow(CartState())
    val state: StateFlow<CartState> = _state.asStateFlow()

    init {
        loadCart()
    }

    fun loadCart() {
        viewModelScope.launch {
            _state.value = _state.value.copy(isLoading = true, error = null)
            val result = repository.getCarrito()
            result.onSuccess { cart ->
                _state.value = _state.value.copy(isLoading = false, cart = cart)
            }.onFailure { error ->
                _state.value = _state.value.copy(
                    isLoading = false,
                    error = error.localizedMessage ?: "Error al cargar carrito"
                )
            }
        }
    }

    fun removeOneItem(libroId: Int) {
        // En una app real llamaríamos a la API para decrementar. 
        // Aquí simulamos o llamamos a addCarritoItem con cantidad -1 si el backend lo permite
        viewModelScope.launch {
            repository.addCarritoItem(libroId, -1).onSuccess { loadCart() }
        }
    }

    fun addOneItem(libroId: Int) {
        viewModelScope.launch {
            repository.addCarritoItem(libroId, 1).onSuccess { loadCart() }
        }
    }

    fun checkout() {
        viewModelScope.launch {
            _state.value = _state.value.copy(isLoading = true)
            // Método de pago 1 = Efectivo/Simulado por ahora
            val result = repository.checkoutCarrito(1, "REF-SIMULADA")
            result.onSuccess {
                _state.value = _state.value.copy(isLoading = false, checkoutSuccess = true, cart = null)
            }.onFailure { error ->
                _state.value = _state.value.copy(
                    isLoading = false,
                    error = error.localizedMessage ?: "Error al procesar compra"
                )
            }
        }
    }
}
