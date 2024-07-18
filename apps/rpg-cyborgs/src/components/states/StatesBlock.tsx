'use client'

import { useAtom } from 'jotai'
import { stateAtomsAtom } from '../../lib/rpg-api/fetcher'
import { ButtonGroup } from '@chakra-ui/react'
import React from 'react'
import StateButton from './StateButton'

function StatesBlock() {
  const [stateAtoms] = useAtom(stateAtomsAtom)
  return (
    <ButtonGroup w={'100%'} alignItems={'stretch'}>
      {stateAtoms.map((state, i) => (
        <StateButton key={i} stateAtom={state} />
      ))}
    </ButtonGroup>
  )
}

export default StatesBlock
