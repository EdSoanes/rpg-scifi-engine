import React from 'react'
import { RpgArg } from '../../../lib/rpg-api/types'
import {
  FormControl,
  FormLabel,
  Input,
  NumberDecrementStepper,
  NumberIncrementStepper,
  NumberInput,
  NumberInputField,
  NumberInputStepper,
} from '@chakra-ui/react'

export declare interface ArgInputProps {
  arg: RpgArg
  onInputCapture: (arg: string, val?: string) => void
}

function ArgInput(props: ArgInputProps) {
  if (props.arg.typeName !== 'Int32' && props.arg.typeName !== 'Dice') {
    return <></>
  }

  return (
    <FormControl>
      <FormLabel htmlFor={props.arg.name}>{props.arg.name}</FormLabel>
      {props.arg.typeName === 'Dice' && (
        <Input
          id={props.arg.name}
          placeholder={props.arg.name}
          onChange={(e) => props.onInputCapture(props.arg.name, e.target.value)}
        />
      )}
      {props.arg.typeName === 'Int32' && (
        <NumberInput
          id={props.arg.name}
          onChange={(valStr) => props.onInputCapture(props.arg.name, valStr)}
        >
          <NumberInputField />
          <NumberInputStepper>
            <NumberIncrementStepper />
            <NumberDecrementStepper />
          </NumberInputStepper>
        </NumberInput>
      )}
    </FormControl>
  )
}

export default ArgInput
