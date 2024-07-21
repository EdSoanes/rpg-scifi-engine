'use client'
import React from 'react'
import { useAtomValue } from 'jotai'
import { Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'
import { playerCharacterAtom } from '../components/atoms/playerCharacter.atom'
import { ActionsBlock } from '../components/actions'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'

export default function CharacterSheet() {
  const playerCharacter = useAtomValue(playerCharacterAtom)

  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <StatsBlock />
      <StatesBlock />
      <ActionsBlock />
      <LifeBlock />
      <GraphStateBlock />
    </>
  )
}
