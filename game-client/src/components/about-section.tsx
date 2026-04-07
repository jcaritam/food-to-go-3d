const details = [
  { label: "Genero", value: "Simulacion de cocina / Delivery" },
  { label: "Plataforma", value: "WebGL — jugable en el navegador" },
  { label: "Estado", value: "En desarrollo activo" },
]

export function AboutSection() {
  return (
    <section className="mt-8 bg-card border border-border/30 rounded-lg p-6">
      <h2 className="text-xs text-muted-foreground uppercase tracking-wider mb-3">Acerca del juego</h2>
      <p className="text-sm text-foreground mb-4">
        Pa' Llevar es un juego de cocina isometrica inspirado en la gastronomia peruana. El jugador
        prepara y sirve platos tipicos antes de que se acabe el tiempo, procesando ingredientes
        en las estaciones de cocina y entregandolos al mostrador.
      </p>
      <ul className="space-y-1">
        {details.map(({ label, value }) => (
          <li key={label} className="flex gap-2 text-sm">
            <span className="text-muted-foreground min-w-[90px]">{label}</span>
            <span className="text-foreground">{value}</span>
          </li>
        ))}
      </ul>
    </section>
  )
}
