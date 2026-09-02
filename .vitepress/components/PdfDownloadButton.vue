<script setup>
import { useData } from 'vitepress'
const { page } = useData()
function print() { window.print() }
</script>

<template>
  <button class="btn" v-if="page.filePath.startsWith('supports')" @click="print" title="Imprimer / Enregistrer en PDF">
    <svg class="fl" xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 32 32">
      <path fill="#313741" d="M28 10H25V4H7v6H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h3v4h18v-4h3a2 2 0 0 0 2-2V12a2 2 0 0 0-2-2M9 6h14v4H9zm14 20H9v-8h14zm5-6h-3v-4H7v4H4V12h24z"/>
      <circle fill="#313741" cx="26" cy="15" r="1"/>
    </svg>
  </button>
</template>

<style scoped>
.fl {
  margin: 1rem 1rem 1rem 1rem;
  float: left;
}
.btn {
  border: 0.1rem solid;
  border-radius: 1rem;
  border-color: rgb(109, 107, 107);
  width: fit-content;
  background: transparent;
  cursor: pointer;
  padding: 0;
}
.btn:hover {
  opacity: 0.5;
}
</style>
