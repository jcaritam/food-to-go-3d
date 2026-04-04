import { useEffect, useState } from "react"
import { fetchChangelog, type ChangelogEntry } from "../lib/changelog"

export function ChangelogSection() {
  const [entries, setEntries] = useState<ChangelogEntry[]>([])

  useEffect(() => {
    fetchChangelog().then(setEntries).catch(() => setEntries([]))
  }, [])

  if (entries.length === 0) return null

  return (
    <section className="mt-8">
      <h2 className="text-xs text-muted-foreground uppercase tracking-wider mb-4">Historial de versiones</h2>
      <div className="space-y-3">
        {entries.map((entry) => (
          <div key={entry.version} className="bg-card border border-border/30 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-2">
              <span className="text-xs font-mono bg-border/50 text-foreground px-2 py-0.5 rounded">
                {entry.version}
              </span>
              <span className="text-xs text-muted-foreground font-mono">{entry.hu}</span>
              <span className="text-xs text-muted-foreground ml-auto">{entry.date}</span>
            </div>
            <p className="text-sm text-foreground font-medium mb-2">{entry.title}</p>
            <ul className="space-y-1">
              {entry.items.map((item, i) => (
                <li key={i} className="text-xs text-muted-foreground flex gap-2">
                  <span>—</span>
                  <span>{item}</span>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>
    </section>
  )
}
