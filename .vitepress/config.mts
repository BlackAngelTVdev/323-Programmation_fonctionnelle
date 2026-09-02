import { defineConfig } from 'vitepress'
import { glob } from 'glob'
import path from 'path'
import { readdirSync, statSync, existsSync, writeFileSync, copyFileSync, mkdirSync, readFileSync } from 'fs'
import type MarkdownIt from 'markdown-it'
import pkg from '../package.json'

// Fail build when siteBase doesn't match the actual GitHub repo name (CI only)
if (process.env.GITHUB_REPOSITORY) {
  const repoName = process.env.GITHUB_REPOSITORY.split('/')[1]
  const expected = `/${repoName}/`
  if (pkg.siteBase !== expected) {
    throw new Error(`[config] siteBase mismatch: package.json="${pkg.siteBase}" vs GITHUB_REPOSITORY implies "${expected}"`)
  }
}

process.env.VITE_EXTRA_EXTENSIONS = 'docx,pdf,csv,xlsx'

// Transforme les liens vers fil-rouge/*/<ex>/ en composant <FilRougeLink> dynamique.
// Le markdown reste navigable en dehors de VitePress (lien statique vers le fil rouge par défaut).
function exoLinksPlugin(md: MarkdownIt) {
  md.core.ruler.push('exo-links', (state) => {
    for (const blockToken of state.tokens) {
      if (blockToken.type !== 'inline' || !blockToken.children) continue
      for (const token of blockToken.children) {
        if (token.type !== 'link_open') continue
        const href = token.attrGet('href') ?? ''
        // trailing-slash exo links (non fil-rouge) → ajoute README.md pour que VitePress traite le lien avec base
        if (/exos\/(?!fil-rouge)[^/]+\/$/.test(href)) {
          token.attrSet('href', href + 'README.md')
        }
      }
    }
  })
}

function filRougeLinksPlugin(md: MarkdownIt) {
  md.core.ruler.push('fil-rouge-links', (state) => {
    for (const blockToken of state.tokens) {
      if (blockToken.type !== 'inline' || !blockToken.children) continue

      const children = blockToken.children
      let i = 0
      while (i < children.length) {
        const token = children[i]
        if (token.type !== 'link_open') { i++; continue }

        const href = token.attrGet('href') ?? ''
        const match = href.match(/fil-rouge\/[^/]+\/(.*)$/)
        if (!match) { i++; continue }

        const ex = match[1]

        // Collecter le texte jusqu'à link_close
        let label = ''
        let j = i + 1
        while (j < children.length && children[j].type !== 'link_close') {
          if (children[j].type === 'text') label += children[j].content
          j++
        }
        label = label || ex

        // Remplacer link_open + contenu + link_close par le composant Vue
        const component = new state.Token('html_inline', '', 0)
        component.content = `<FilRougeLink ex="${ex}" label="${label.replace(/"/g, '&quot;')}" />`
        children.splice(i, j - i + 1, component)
        // i inchangé — on reparse depuis la même position (maintenant occupée par le composant)
      }
    }
  })
}

const filRougesData = existsSync('exos/fil-rouge')
  ? readdirSync('exos/fil-rouge')
      .filter(d => statSync(`exos/fil-rouge/${d}`).isDirectory())
      .map(id => {
        const base = `exos/fil-rouge/${id}`
        const subdirs = readdirSync(base).filter(d => statSync(`${base}/${d}`).isDirectory())

        // Dirs without README.md are data dirs — auto-generate (or refresh) a download page
        const dataDirs = subdirs
          .filter(d => !existsSync(`${base}/${d}/README.md`))
          .map(d => ({ name: d, files: readdirSync(`${base}/${d}`).filter(f => statSync(`${base}/${d}/${f}`).isFile() && f !== 'index.md') }))
          .filter(({ files }) => files.length > 0)

        for (const dir of dataDirs) {
          const links = dir.files.map(f => `- <a href="${f}" download>${f}</a>`).join('\n')
          writeFileSync(`${base}/${dir.name}/index.md`, `# ${dir.name}\n\n${links}\n`)
        }

        // After generation, all subdirs with README.md or index.md are exercises
        const exercises = subdirs.filter(d =>
          existsSync(`${base}/${d}/README.md`) || existsSync(`${base}/${d}/index.md`)
        )

        return { id, exercises, dataDirs }
      })
  : []

const supportsNavItems = glob.sync('supports/**/*.md', { posix: true })
  .filter(f => !f.endsWith('references.md'))
  .sort()
  .map(f => ({ text: path.basename(f).replace('.md', ''), link: '/' + f.replace('.md', '') }))

const repoUrl = process.env.GITHUB_REPOSITORY
  ? `https://github.com/${process.env.GITHUB_REPOSITORY}`
  : 'https://github.com/ETML-INF/323-Programmation_fonctionnelle'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  title: "ICT-323 Fun",
  description: "Module ICT 323 sur la programmation fonctionnelle",

  markdown: {
    config: (md) => {
      filRougeLinksPlugin(md)
      exoLinksPlugin(md)
    }
  },

  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Thématiques', link: '/thematiques/01-paradigmes-fonctionnels' },
      { text: 'Supports', items: supportsNavItems },
      { text: 'Références', link: '/supports/source/references' }
    ],

    sidebar: [
      {
        text: 'Thématiques',
        collapsed : false,
        items: glob.sync('thematiques/*.md', { posix: true })
          .sort()
          .map(f => {
            const num = path.basename(f).match(/^(\d+)/)?.[1]
            const h1 = readFileSync(f, 'utf-8').match(/^#\s+(.*)/m)?.[1]
            const title = h1 ?? path.basename(f).replace('.md', '')
            return { text: num ? `${num} — ${title}` : title, link: '/' + f.replace('.md', '') }
          })
      },
      {
        text: 'Documentation technique',
        collapsed: false,
        items: [{ text: 'Références LINQ', link: '/supports/source/references' }]
      },
      {
        text: 'Supports',
        collapsed : true,
        items: glob.sync('supports/**/*.md',{posix:true})
          .filter(f => !f.endsWith('references.md'))
          .map(f => '/' + f)
          .map((file) => ({ text: `${path.basename(file).replace(".md","")}`, link: `${file}` })).reverse()
      },
      {
        text: 'Activités fil rouge',
        collapsed: true,
        items: filRougesData.map(fr => ({
          text: fr.id,
          collapsed: false,
          link: `/exos/fil-rouge/${fr.id}/`,
          items: fr.exercises.map(ex => ({
            text: ex,
            link: `/exos/fil-rouge/${fr.id}/${ex}/`
          }))
        }))
      },
      {
        text: 'Exercices divers',
        collapsed: true,
        items: glob.sync(['exos/*/README.md','exos/*/enoncé.md'],{posix:true})
          .filter(f => !f.startsWith('exos/fil-rouge/'))
          .map(f => {
            const parts = f.split('/')
            const file = parts[parts.length - 1]
            const dir = `/${parts.slice(0, -1).join('/')}/`
            return { text: parts[1], link: file === 'README.md' ? dir : dir + file.replace('.md', '') }
          })
          .reverse()
      },
    ],

    socialLinks: [
      { icon: 'github', link: repoUrl }
    ],
    search: {
      provider: 'local'
    }
  },

  ignoreDeadLinks: [
    /\/slides\//,                      // Slidev output — not VitePress pages
    /\.(pdf|xlsx|docx|csv|pptx|cs|html)$/i,  // static assets VitePress doesn't process
    /\/assets\/SearchSpeed/,           // C# demo project directory, no index page
    /\/gpx\//,                         // GPX data directory, no index page
    /^\.\/(billboard|crawler\/index)$/, // swapi static HTML templates (VitePress strips .html before checking)
  ],
  base: pkg.siteBase,
  srcExclude: ['slides/**'],

  rewrites: {
    'README.md': 'index.md',
    'exos/:name/README.md': 'exos/:name/index.md',
    'exos/fil-rouge/:ctx/README.md': 'exos/fil-rouge/:ctx/index.md',
    'exos/fil-rouge/:ctx/:ex/README.md': 'exos/fil-rouge/:ctx/:ex/index.md',
  },

  buildEnd: async (siteConfig) => {
    const filRougeDir = 'exos/fil-rouge'
    if (!existsSync(filRougeDir)) {
      console.warn('\n[fil-rouge] Dossier exos/fil-rouge/ introuvable — aucune validation\n')
      return
    }

    if (filRougesData.length === 0) {
      console.warn('\n[fil-rouge] Aucun fil rouge trouvé dans exos/fil-rouge/\n')
      return
    }

    // Validate: every exercise subdir must have README.md or index.md
    const errors: string[] = []
    for (const fr of filRougesData) {
      for (const ex of fr.exercises) {
        const hasPage = existsSync(`${filRougeDir}/${fr.id}/${ex}/README.md`)
                     || existsSync(`${filRougeDir}/${fr.id}/${ex}/index.md`)
        if (!hasPage) errors.push(`  ✗ ${filRougeDir}/${fr.id}/${ex}/`)
      }
    }
    if (errors.length) {
      console.error('\n[fil-rouge] Pages manquantes :\n' + errors.join('\n') + '\n')
      process.exit(1)
    }

    // Copy data files to output so they are downloadable in production
    for (const fr of filRougesData) {
      for (const dir of fr.dataDirs) {
        const destDir = path.join(siteConfig.outDir, 'exos/fil-rouge', fr.id, dir.name)
        mkdirSync(destDir, { recursive: true })
        for (const file of dir.files) {
          copyFileSync(`${filRougeDir}/${fr.id}/${dir.name}/${file}`, path.join(destDir, file))
        }
      }
    }

    console.log(`\n[fil-rouge] ${filRougesData.length} fil(s) rouge validé(s) : ${filRougesData.map(f => f.id).join(', ')} ✓\n`)

    // Copy standalone PDFs from supports/ (those without a markdown source in supports/source/)
    // PDFs with a .md counterpart are legacy generated files — skipped (HTML rendering replaces them)
    const supportsDir = 'supports'
    if (existsSync(supportsDir)) {
      const pdfs = readdirSync(supportsDir).filter(f => {
        if (!f.endsWith('.pdf') || !statSync(`${supportsDir}/${f}`).isFile()) return false
        const stem = f.replace(/\.pdf$/, '')
        return !existsSync(`${supportsDir}/source/${stem}.md`)
      })
      if (pdfs.length > 0) {
        const destDir = path.join(siteConfig.outDir, 'supports')
        mkdirSync(destDir, { recursive: true })
        for (const f of pdfs) {
          copyFileSync(`${supportsDir}/${f}`, path.join(destDir, f))
        }
        console.log(`\n[supports] ${pdfs.length} PDF(s) autonome(s) copié(s) : ${pdfs.join(', ')} ✓\n`)
      }
    }
  }
})
