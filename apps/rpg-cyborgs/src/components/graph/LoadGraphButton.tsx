'use client'

import { useAtom } from 'jotai'
import { getGraphState } from '../../lib/rpg-api/fetcher'
import { graphStateAtom } from '../atoms/graphState.atom'
import React from 'react'
import { Button } from '@chakra-ui/react'

function LoadGraphButton() {
  const [, setGraphState] = useAtom(graphStateAtom)

  const fetchGraphState = async () => {
    const response = await getGraphState('Benny')
    setGraphState(response?.graphState)
  }

  return (
    <div>
      <Button colorScheme="teal" variant="outline" onClick={fetchGraphState}>
        Get Benny
      </Button>
    </div>
  )
}

export default LoadGraphButton
