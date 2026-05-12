package com.example.movilwed3.data.remote.dto

data class LoginRequest(val email: String, val password: String)
data class RegisterRequest(val nombreCompleto: String, val email: String, val password: String)
data class AuthResponse(val accessToken: String, val refreshToken: String, val email: String, val nombreCompleto: String, val rol: String)

data class UsuarioDto(val id: Int, val nombreCompleto: String, val email: String, val rol: String, val activo: Boolean, val puntos: Int)

data class LibroDto(
    val id: Int,
    val titulo: String,
    val autor: AutorDto?,
    val categoria: CategoriaDto?,
    val isbn: String,
    val cantidadDisponible: Int,
    val precio: Double,
    val imagenUrl: String?,
    val introduccion: String?,
    val descripcion: String?
)

data class AutorDto(val id: Int, val nombre: String)
data class CategoriaDto(val id: Int, val nombre: String)

data class PrestamoCreateRequest(val diasPrestamo: Int, val lineas: List<LineaPrestamoDto>)
data class LibroMiniDto(val id: Int, val titulo: String, val autor: AutorDto?)

data class ReservaCreateRequest(val fechaPrevista: String, val lineas: List<LineaReservaDto>)
data class LineaReservaDto(val libroId: Int, val cantidad: Int)

data class LibroReservaDto(
    val id: Int,
    val titulo: String,
    val autor: String?
)

data class ReservaDto(
    val id: Int,
    val libro: LibroReservaDto?,
    val fechaReserva: String,
    val fechaExpiracion: String?,
    val estado: Int,
    val estadoNombre: String
)

data class MultaDto(val id: Int, val prestamoId: Int, val monto: Double, val motivo: String, val fechaGeneracion: String, val pagada: Boolean)
data class PagoQrRequest(val multaId: Int, val comprobanteBase64: String)

data class FavoritoDto(val id: Int, val libroId: Int, val libro: LibroMiniDto?)

data class FcmTokenRequest(val fcmToken: String, val device: String?)
data class NotificacionDto(val id: Int, val titulo: String, val mensaje: String, val tipo: String, val fechaCreacion: String, val leida: Boolean)

data class CarritoDto(val id: Int, val items: List<CarritoItemDto>, val total: Double)
data class CarritoItemDto(val id: Int, val libroId: Int, val titulo: String, val precioUnitario: Double, val cantidad: Int, val subtotal: Double)
data class AddCarritoItemRequest(val libroId: Int, val cantidad: Int)

data class VentaCreateRequest(val metodoPagoId: Int, val comprobanteReferencia: String?)
