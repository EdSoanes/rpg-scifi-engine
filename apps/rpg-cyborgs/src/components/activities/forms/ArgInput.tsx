import { RpgArg } from '../../../lib/rpg-api/types'
// import {
//   Input,
//   Fieldset,
//   Field,
//   // NumberInputInput
//   // NumberDecrementStepper,
//   // NumberIncrementStepper,
//   // NumberInput,
//   // NumberIn
//   // NumberInputStepper,
// } from '@chakra-ui/react'

import { Fieldset, Input } from '@chakra-ui/react'
import { Field } from '../../ui/field'
import {
  NumberInputField,
  NumberInputLabel,
  NumberInputRoot,
} from '../../ui/number-input'

export declare interface ArgInputProps {
  arg: RpgArg
  onInputCapture: (arg: string, val?: string) => void
}

function ArgInput(props: ArgInputProps) {
  const { arg, onInputCapture } = props
  if (arg.type !== 'Int32' && arg.type !== 'Dice') {
    return <></>
  }

  return (
    <Fieldset.Root>
      <Fieldset.Content></Fieldset.Content>

      {arg.type === 'Dice' && (
        <Field label={arg.name}>
          <Input
            id={props.arg.name}
            placeholder={props.arg.name}
            onChange={(e) => onInputCapture(props.arg.name, e.target.value)}
          />
        </Field>
      )}
      {props.arg.type === 'Int32' && (
        <Field label={arg.name}>
          <NumberInputRoot
            defaultValue="10"
            onValueChange={({ value }) => {
              field.onChange(value)
            }}
            onChange={(e) => onInputCapture(arg.name, e.target.value)}
          >
            <NumberInputField />
          </NumberInputRoot>
        </Field>
      )}
    </Fieldset.Root>
  )
}

export default ArgInput
