'use client'

import { atom } from 'jotai'
import { StatGroup } from '@chakra-ui/react'
import React from 'react'
import { PropValue } from '../../lib/rpg-api/types'
import StatPanel from './StatPanel'
import { playerCharacterAtom } from '../atoms/playerCharacter.atom'

const strengthAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.strength ?? null
)
const agilityAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.agility ?? null
)
const healthAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.health ?? null
)
const brainsAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.brains ?? null
)
const insightAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.insight ?? null
)
const charismaAtom = atom<PropValue | null>(
  (get) => get(playerCharacterAtom)?.charisma ?? null
)

function StatsBlock() {
  return (
    <StatGroup w={'100%'} alignItems={'stretch'}>
      <StatPanel
        propName="Strength"
        propNameAbbr="STR"
        propValueAtom={strengthAtom}
      />
      <StatPanel
        propName="Agility"
        propNameAbbr="AGI"
        propValueAtom={agilityAtom}
      />
      <StatPanel
        propName="Health"
        propNameAbbr="HEL"
        propValueAtom={healthAtom}
      />
      <StatPanel
        propName="Brains"
        propNameAbbr="BRA"
        propValueAtom={brainsAtom}
      />
      <StatPanel
        propName="Insight"
        propNameAbbr="INS"
        propValueAtom={insightAtom}
      />
      <StatPanel
        propName="Charisma"
        propNameAbbr="CHA"
        propValueAtom={charismaAtom}
      />
    </StatGroup>
  )
}

export default StatsBlock
