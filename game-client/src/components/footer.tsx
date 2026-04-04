import { VERSION_LABEL } from "../lib/version"

export const Footer = () => {
  return (
    <footer className="border-t border-border/30">
      <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
        <p className="text-xs text-muted-foreground">
          Desarrollado por{" "}
          <a
            href="https://github.com/jcaritam"
            target="_blank"
            rel="noopener noreferrer"
            className="text-muted-foreground hover:underline hover:text-white"
          >
            @jcaritam
          </a>
        </p>
        <p className="text-xs text-muted-foreground font-mono">{VERSION_LABEL}</p>
      </div>
    </footer>
  )
}
