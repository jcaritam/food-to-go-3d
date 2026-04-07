const team = [
  {
    name: "Joseph Antonio Carita Mamani",
    github: "https://github.com/jcaritam",
    handle: "@jcaritam",
  },
]

export function CreditsSection() {
  return (
    <section className="mt-8 bg-card border border-border/30 rounded-lg p-6">
      <h2 className="text-xs text-muted-foreground uppercase tracking-wider mb-4">Equipo</h2>
      <ul className="space-y-3">
        {team.map(({ name, github, handle }) => (
          <li key={handle} className="flex items-center justify-between text-sm">
            <span className="text-foreground">{name}</span>
            <a
              href={github}
              target="_blank"
              rel="noopener noreferrer"
              className="text-muted-foreground hover:text-white hover:underline font-mono text-xs"
            >
              {handle}
            </a>
          </li>
        ))}
      </ul>
      <p className="text-xs text-muted-foreground mt-4">
        Universidad — Desarrollo de Videojuegos
      </p>
    </section>
  )
}
