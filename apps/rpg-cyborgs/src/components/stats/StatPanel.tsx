import { PropValue } from '@lib/rpg-api/cyborg-types'
import PropertyValue from '@components/prop/property-value'
import { overridePropValue } from '@app/thunks'
import { useAppDispatch } from '@app/hooks'

export declare interface StatPanelProps {
  name: string
  abbreviatedName: string
  prop: string
  propValue?: PropValue
}

function StatPanel(props: StatPanelProps) {
  const { name, prop, propValue } = props
  const dispatch = useAppDispatch()

  const onPropValueChanged = async (
    value: number,
    propValue?: PropValue
  ): Promise<void> => {
    if (propValue?.ownerId && name) {
      console.log(`Updating propValue ${name}`, value)
      await dispatch(
        overridePropValue({
          propRef: { entityId: propValue.id, prop: prop },
          overrideValue: value,
        })
      )
    }
  }

  const onPropShowDetails = (name: string, propValue: PropValue) => {
    console.log(`Show Details ${name}`, propValue)
  }

  return (
    propValue && (
      <PropertyValue
        name={name}
        prop={prop}
        propValue={propValue}
        onPropValueChanged={async (value) =>
          await onPropValueChanged(value, propValue)
        }
        onShowDetails={() => onPropShowDetails(name, propValue)}
      />
    )
  )
}

export default StatPanel
