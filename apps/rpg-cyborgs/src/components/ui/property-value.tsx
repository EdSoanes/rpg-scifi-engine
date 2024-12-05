import { Grid, GridItem, HTMLChakraProps, Text } from '@chakra-ui/react'
import { NumberInputField, NumberInputRoot } from './number-input'
import { PropValue } from '@/lib/rpg-api/cyborg-types'
import { useMemo, useState } from 'react'
import { propertyValue } from './property-value.css'
import { show, hide } from '../../styles/global.css'

export interface PropertyValueProps extends HTMLChakraProps<'div'> {
  name: string
  abbreviatedName: string
  propValue: PropValue
  onPropValueChanged: (value: number) => void
}

export default function PropertyValue(props: PropertyValueProps) {
  const { abbreviatedName, propValue, onPropValueChanged } = props
  const [visibility, setVisibility] = useState<string>(hide)

  const val = useMemo(() => String(propValue?.value), [propValue?.value])
  console.log('propValue', propValue)
  // const eq = (propValue?.value ?? 0) === (propValue?.baseValue ?? 0)
  // const inc = !eq && (propValue?.value ?? 0) > (propValue?.baseValue ?? 0)
  // const dec = !eq && (propValue?.value ?? 0) < (propValue?.baseValue ?? 0)

  return (
    <Grid
      className={propertyValue}
      onMouseOver={() => setVisibility(show)}
      onMouseLeave={() => setVisibility(hide)}
    >
      <GridItem>
        <Text>{abbreviatedName}</Text>
      </GridItem>
      <GridItem>
        {propValue?.value && (
          <NumberInputRoot
            maxW="200px"
            size={'md'}
            value={val}
            onValueChange={(e) => onPropValueChanged(Number(e.value))}
          >
            <NumberInputField />
          </NumberInputRoot>
        )}
      </GridItem>
      <GridItem className={visibility}>something</GridItem>
    </Grid>
  )
}
