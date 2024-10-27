'use client'

import React from 'react'
import { Button } from '@chakra-ui/react'
import { useAppDispatch } from '../../app/hooks'
import { fetchGraphState } from '../../app/thunks'

function LoadGraphButton() {

  const dispatch = useAppDispatch()

  const onLoadGraphState = async () => {
    dispatch(fetchGraphState('Benny'))
  }

  return (
    <div>
      <Button colorScheme="teal" variant="outline" onClick={onLoadGraphState}>
        Get Benny
      </Button>
    </div>
  )
}

export default LoadGraphButton
