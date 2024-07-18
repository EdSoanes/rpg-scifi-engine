'use client'
import React from 'react'
import { playerCharacterAtom, graphStateAtom } from '../lib/rpg-api/fetcher'
import { useAtomValue } from 'jotai'
import { Code, Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'

export default function CharacterSheet() {
  const playerCharacter = useAtomValue(playerCharacterAtom)
  const graphState = useAtomValue(graphStateAtom)

  return (
    <>
      <Heading>{playerCharacter?.name ?? 'Nobody'}</Heading>
      <StatsBlock />
      <StatesBlock />
      <Code>{JSON.stringify(graphState, undefined, 2)}</Code>
    </>
  )
}
