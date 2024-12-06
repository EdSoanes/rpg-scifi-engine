import {
  useDisclosure,
  Text,
  Grid,
  GridItem,
  Heading,
  HStack,
} from '@chakra-ui/react'
import { useState } from 'react'
import { PropDescription } from '../../lib/rpg-api/types'
import { getPropDesc } from '../../lib/rpg-api/fetcher'
import { selectGraphState } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { PropValue } from '../../lib/rpg-api/cyborg-types'
import PropertyValue from '../ui/property-value'
import { overridePropValue } from '../../app/thunks'
import { useAppDispatch } from '../../app/hooks'

//const describeAtom = atom<PropDesc | undefined>(undefined)

export declare interface StatPanelProps {
  propName: string
  propNameAbbr: string
  propValue?: PropValue
}

function StatPanel(props: StatPanelProps) {
  const { propName, propNameAbbr, propValue } = props
  const graphState = useSelector(selectGraphState)
  const [describe, setDescribe] = useState<PropDescription | null | undefined>()
  const dispatch = useAppDispatch()

  const onDescribe = async () => {
    if (props?.propValue) {
      const response = await getPropDesc(
        props.propValue.id,
        'Value',
        graphState!
      )
      setDescribe(response?.data)
      onOpen()
    }
  }

  const onPropValueChanged = async (
    value: number,
    propValue?: PropValue
  ): Promise<void> => {
    if (propValue?.ownerId && propValue?.name) {
      console.log(`Updating propValue ${propValue.name}`, value)
      await dispatch(
        overridePropValue({
          propRef: { entityId: propValue.id, prop: 'Value' },
          value: value,
        })
      )
    }
  }
  const { onOpen, onClose } = useDisclosure()

  return (
    <HStack>
      <Heading size={'md'}>{propNameAbbr}</Heading>
      <PropertyValue
        propValue={propValue}
        onPropValueChanged={async (value) =>
          await onPropValueChanged(value, propValue)
        }
      />
    </HStack>
  )
}

export default StatPanel
