'use client'
import React from 'react'
import { Heading } from '@chakra-ui/react'
import { StatsBlock } from '../components/stats'
import { StatesBlock } from '../components/states'
import { ActionsBlock } from '../components/actions'
import LifeBlock from '../components/life/LifeBlock'
import { GraphStateBlock } from '../components/graph'
import { useSelector } from 'react-redux'
import { selectPlayerCharacter } from '../app/graphState/graphSelectors'

export default function CharacterSheet() {
  const playerCharacter = useSelector(selectPlayerCharacter)

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
