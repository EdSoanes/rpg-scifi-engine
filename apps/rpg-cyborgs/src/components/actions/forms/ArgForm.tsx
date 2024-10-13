import React from 'react'
import { Button, Stack } from '@chakra-ui/react'
import { RpgArg } from '../../../lib/rpg-api/types'
import ArgInput from './ArgInput'

export declare interface ArgFormProps {
  argSet: RpgArg[]
  onSubmit: (argValues: { [key: string]: string | null | undefined }) => void
}

export default function ArgForm(props: ArgFormProps) {
  const argVals: { [key: string]: string | null | undefined } = {}

  const onInputCapture = (arg: string, val?: string) => {
    argVals[arg] = val
  }

  const onSubmit = () => {
    props.onSubmit(argVals)
  }

  return (
    <Stack>
      {props.argSet.map((a, i) => (
        <ArgInput key={i} arg={a} onInputCapture={onInputCapture} />
      ))}

      <Button mt={4} colorScheme="teal" type="submit" onClick={onSubmit}>
        Submit
      </Button>
    </Stack>
  )
}
