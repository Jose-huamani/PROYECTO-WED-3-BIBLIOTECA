package com.example.movilwed3.data.remote.api

import com.example.movilwed3.data.remote.dto.*
import retrofit2.Response
import retrofit2.http.*

interface LibraryApi {

    // Auth
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): Response<AuthResponse>

    @POST("auth/register")
    suspend fun register(@Body request: RegisterRequest): Response<AuthResponse>

    // Profile
    @GET("usuarios/me")
    suspend fun getProfile(): Response<UsuarioDto>

    @PUT("usuarios/me")
    suspend fun updateProfile(@Body request: UpdateProfileRequest): Response<Unit>

    // Books
    @GET("libros")
    suspend fun getLibros(): Response<List<LibroDto>>

    @GET("libros/{id}")
    suspend fun getLibro(@Path("id") id: Int): Response<LibroDto>

    // Loans
    @POST("prestamos")
    suspend fun solicitarPrestamo(@Body request: PrestamoCreateRequest): Response<Unit>

    @GET("prestamos")
    suspend fun getMisPrestamos(): Response<List<LoanDto>>

    // Reservations
    @POST("reservas/{libroId}")
    suspend fun solicitarReserva(@Path("libroId") libroId: Int): Response<Unit>

    @GET("reservas")
    suspend fun getMisReservas(): Response<List<ReservaDto>>

    // Fines
    @GET("multas")
    suspend fun getMisMultas(): Response<List<MultaDto>>

    @POST("multas/pagar-qr")
    suspend fun pagarMultaQR(@Body request: PagoQrRequest): Response<Unit>

    // Favorites
    @GET("favoritos")
    suspend fun getFavoritos(): Response<List<FavoritoDto>>

    @POST("favoritos/{libroId}")
    suspend fun addFavorito(@Path("libroId") libroId: Int): Response<Unit>

    @DELETE("favoritos/{libroId}")
    suspend fun removeFavorito(@Path("libroId") libroId: Int): Response<Unit>

    // Notifications FCM
    @POST("notifications/register-token")
    suspend fun registerFcmToken(@Body request: FcmTokenRequest): Response<Unit>

    @GET("notifications/user")
    suspend fun getNotificaciones(): Response<List<NotificacionDto>>
    
    // Store & Cart
    @GET("carrito")
    suspend fun getCarrito(): Response<CarritoDto>
    
    @POST("carrito/agregar/{libroId}")
    suspend fun addCarritoItem(
        @Path("libroId") libroId: Int,
        @Query("cantidad") cantidad: Int
    ): Response<Unit>
    
    @POST("ventas/comprar-carrito")
    suspend fun checkoutCarrito(@Body request: VentaCreateRequest): Response<Unit>
}
