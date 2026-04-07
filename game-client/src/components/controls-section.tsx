const groups = [
  {
    label: "Movimiento",
    controls: [
      { key: "W", desc: "Adelante" },
      { key: "A", desc: "Izquierda" },
      { key: "S", desc: "Atras" },
      { key: "D", desc: "Derecha" },
    ],
  },
  {
    label: "Accion",
    controls: [
      { key: "E", desc: "Seleccionar objeto" },
      { key: "Q", desc: "Picar" },
    ],
  },
  {
    label: "Extra",
    controls: [
      { key: "Espacio", desc: "Aumentar velocidad" },
    ],
  },
]

export function ControlsSection() {
  return (
    <section className="mt-8">
      <h2 className="text-xs text-muted-foreground uppercase tracking-wider mb-4">Controles</h2>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {groups.map((group) => (
          <div key={group.label} className="bg-card border border-border/30 rounded-lg p-4">
            <div className="text-xs text-muted-foreground uppercase tracking-wider mb-3">
              {group.label}
            </div>
            <ul className="space-y-2">
              {group.controls.map(({ key, desc }) => (
                <li key={key} className="flex items-center gap-3 text-sm">
                  <kbd className="px-2 py-0.5 rounded bg-border/50 text-foreground font-mono text-xs min-w-[2rem] text-center">
                    {key}
                  </kbd>
                  <span className="text-muted-foreground">{desc}</span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </section>
  )
}
