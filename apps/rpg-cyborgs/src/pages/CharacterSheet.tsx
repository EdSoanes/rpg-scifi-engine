'use client'
import React from 'react'
import { useAtomValue } from 'jotai'
import { Code, Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'
import { playerCharacterAtom } from '../components/atoms/playerCharacter.atom'
import { graphStateAtom } from '../components/atoms/graphState.atom'
import { ActionsBlock } from '../components/actions'
import LifeBlock from '../components/life/LifeBlock'

export default function CharacterSheet() {
  const playerCharacter = useAtomValue(playerCharacterAtom)
  const graphState = useAtomValue(graphStateAtom)

  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <StatsBlock />
      <ActionsBlock />
      <LifeBlock />
      <StatesBlock />
      <Code>{JSON.stringify(graphState, undefined, 2)}</Code>
    </>
  )
}
