<script setup lang="ts">
import { useFilRouge } from '../composables/useFilRouge'

const { active, set } = useFilRouge()

// Liste des fil rouges disponibles — à terme auto-découverts depuis exos/fil-rouge/
const options = [
  { id: 'esport',     label: 'Esport Advanced' },
  { id: 'esportEasy', label: 'Esport Easy' },
  { id: 'meteo',      label: 'Météo' },
]
</script>

<template>
  <div class="fil-rouge-selector">
    <span class="label">Fil rouge :</span>
    <select :value="active" @change="set(($event.target as HTMLSelectElement).value)">
      <option v-for="o in options" :key="o.id" :value="o.id">{{ o.label }}</option>
    </select>
  </div>
</template>

<style scoped>
.fil-rouge-selector {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  padding: 0 12px;
}
.label {
  color: var(--vp-c-text-2);
  white-space: nowrap;
}
select {
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-border);
  border-radius: 4px;
  color: var(--vp-c-text-1);
  padding: 2px 6px;
  font-size: 13px;
  cursor: pointer;
}
select:hover {
  border-color: var(--vp-c-brand-1);
}
</style>
