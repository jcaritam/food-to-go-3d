export type ChangelogEntry = {
  version: string
  date: string
  hu: string
  title: string
  items: string[]
}

export async function fetchChangelog(): Promise<ChangelogEntry[]> {
  const res = await fetch("/CHANGELOG.md")
  const text = await res.text()
  return parseChangelog(text)
}

function parseChangelog(raw: string): ChangelogEntry[] {
  const entries: ChangelogEntry[] = []
  const blocks = raw.split(/^## /m).filter(Boolean)

  for (const block of blocks) {
    const lines = block.trim().split("\n")
    const headerMatch = lines[0].match(/\[(.+?)\]\s*[—-]\s*(.+)/)
    if (!headerMatch) continue

    const version = headerMatch[1].trim()
    const date = headerMatch[2].trim()

    const huLine = lines.find((l) => l.startsWith("### "))
    if (!huLine) continue

    const huMatch = huLine.replace("### ", "").match(/^(HU-\d+)\s*[—-]\s*(.+)/)
    if (!huMatch) continue

    const hu = huMatch[1].trim()
    const title = huMatch[2].trim()
    const items = lines
      .filter((l) => l.startsWith("- "))
      .map((l) => l.replace(/^- /, "").trim())

    entries.push({ version, date, hu, title, items })
  }

  return entries
}
