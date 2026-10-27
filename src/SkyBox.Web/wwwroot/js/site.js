"use strict";

document.querySelectorAll("[data-print]").forEach(button => button.addEventListener("click", () => window.print()));

const booking = document.querySelector("[data-booking]");
if (booking) {
    const seats = [...booking.querySelectorAll('input[name="SelectedSeats"]:not(:disabled)')];
    const error = booking.querySelector("[data-seat-error]");
    const price = Number(booking.dataset.price);
    const money = new Intl.NumberFormat("es-BO", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const update = () => {
        const selected = seats.filter(seat => seat.checked);
        booking.querySelector("[data-seat-list]").textContent = selected.length ? selected.map(seat => seat.value).join(" · ") : "Selecciona en el mapa";
        booking.querySelector("[data-seat-count]").textContent = `${selected.length} ${selected.length === 1 ? "entrada" : "entradas"}`;
        booking.querySelector("[data-total]").textContent = money.format(selected.length * price);
        error.textContent = "";
    };
    seats.forEach(seat => seat.addEventListener("change", () => {
        if (seats.filter(item => item.checked).length > 8) {
            seat.checked = false;
            update();
            error.textContent = "Puedes reservar hasta 8 asientos por grupo.";
        } else update();
    }));
    booking.addEventListener("submit", event => {
        if (!seats.some(seat => seat.checked)) {
            event.preventDefault();
            error.textContent = "Elige al menos un asiento para continuar.";
            seats[0]?.focus();
        }
    });
    update();
}
