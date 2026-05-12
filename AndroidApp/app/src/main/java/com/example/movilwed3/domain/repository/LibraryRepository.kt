package com.example.movilwed3.domain.repository

import com.example.movilwed3.data.remote.dto.*

interface LibraryRepository {
    suspend fun login(request: LoginRequest): Result<AuthResponse>
    suspend fun register(request: RegisterRequest): Result<AuthResponse>
    
    suspend fun getProfile(): Result<UsuarioDto>
    suspend fun updateProfile(request: UpdateProfileRequest): Result<Unit>
    
    suspend fun getLibros(): Result<List<LibroDto>>
    suspend fun getLibro(id: Int): Result<LibroDto>
    
    suspend fun getFavoritos(): Result<List<FavoritoDto>>
    suspend fun addFavorito(libroId: Int): Result<Unit>
    
    suspend fun solicitarPrestamo(libroId: Int, dias: Int): Result<Unit>
    suspend fun solicitarReserva(libroId: Int): Result<Unit>
    suspend fun getMisReservas(): Result<List<ReservaDto>>
    suspend fun getMisPrestamos(): Result<List<LoanDto>>
    
    suspend fun getMisMultas(): Result<List<MultaDto>>
    suspend fun pagarMultaQR(multaId: Int, comprobante: String): Result<Unit>
    
    suspend fun registerFcmToken(token: String): Result<Unit>
    suspend fun getNotificaciones(): Result<List<NotificacionDto>>
    
    suspend fun getCarrito(): Result<CarritoDto>
    suspend fun addCarritoItem(libroId: Int, cantidad: Int = 1): Result<Unit>
    suspend fun checkoutCarrito(metodoPagoId: Int, comprobante: String?): Result<Unit>
}
